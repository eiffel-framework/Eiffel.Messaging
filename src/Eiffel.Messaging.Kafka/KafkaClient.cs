using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Eiffel.Messaging.Abstractions;
using Microsoft.Extensions.Logging;

namespace Eiffel.Messaging.Kafka
{
    public class KafkaClient : IMessageBrokerClient, IDisposable
    {
        private readonly ILogger<KafkaClient> _logger;
        private readonly IMessageRouteRegistry _messageRouteRegistry;
        private readonly IMessageSerializer _messageSerializer;

        private readonly KafkaClientConfig _config;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public KafkaClient(ILogger<KafkaClient> logger, 
            KafkaClientConfig config, 
            IMessageRouteRegistry messageRouteRegistry,
            IMessageSerializer messageSerializer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            
            _messageRouteRegistry = messageRouteRegistry ?? throw new ArgumentNullException(nameof(messageRouteRegistry));
            _messageSerializer = messageSerializer ?? throw new ArgumentNullException(nameof(messageSerializer));

            _cancellationTokenSource = new CancellationTokenSource();

            _config.ProducerConfig.ClientId = Dns.GetHostName();
            _config.ConsumerConfig.ClientId = Dns.GetHostName();
        }

        /// <summary>
        /// Consume message from message broker
        /// <seealso cref="IMessageRouteRegistry"/>
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="ConsumeException" />
        public virtual Task ConsumeAsync<TMessage>(Action<TMessage> dispatcher, CancellationToken cancellationToken = default) 
            where TMessage : class
        {
            using (var consumer = new ConsumerBuilder<Null, byte[]>(_config.ConsumerConfig).Build())
            {
                var sourceTopic = _messageRouteRegistry.GetRoute<TMessage>();

                consumer.Subscribe(sourceTopic);

                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        consumer.Unsubscribe();
                        throw new OperationCanceledException();
                    }

                    try
                    {
                        var consumeResult = consumer.Consume(cancellationToken);

                        if (consumeResult.Message?.Value?.Length > 0)
                        {
                            var payload = _messageSerializer.Deserialize<TMessage>(consumeResult.Message.Value);

                            dispatcher.Invoke(payload);

                            consumer.Commit(consumeResult);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, $"{_config.Name} consume failed for {typeof(TMessage).Name} :: {sourceTopic}");
                        consumer.Close();
                    }
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sends message to message broker
        /// <seealso cref="IMessageRouteRegistry"/>
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="ProduceException{TKey, TValue}" />
        public virtual Task ProduceAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) 
            where TMessage : class
        {
            var targetTopic = GetMessageRoute<TMessage>();

            using (var producer = new ProducerBuilder<Null, byte[]>(_config.ProducerConfig).Build())
            {
                var payload = new Message<Null, byte[]>()
                {
                    Value = _messageSerializer.Serialize(message),
                };

                producer.Produce(targetTopic, payload);

                producer.Flush(cancellationToken);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Canacel all subscribtions
        /// </summary>
        public virtual void Unsubscribe()
        {
            _cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  Dispose cancel all consumers
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            _cancellationTokenSource.Dispose();
        }

        private string GetMessageRoute<TMessage>()
            where TMessage : class
        {
            return _messageRouteRegistry.GetRoute<TMessage>();
        }
    }
}
