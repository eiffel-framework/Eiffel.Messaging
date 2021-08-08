using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Eiffel.Messaging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eiffel.Messaging.Kafka
{
    public class KafkaClient : IMessageBrokerClient, IDisposable
    {
        private readonly ILogger<KafkaClient> _logger;
        private readonly IOptions<KafkaClientConfig> _config;
        private readonly IMessageRouteRegistry _messageRouteRegistry;
        private readonly IMessageSerializer _messageSerializer;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public KafkaClient(ILogger<KafkaClient> logger, 
            IOptions<KafkaClientConfig> config, 
            IMessageRouteRegistry messageRouteRegistry,
            IMessageSerializer messageSerializer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            
            _messageRouteRegistry = messageRouteRegistry ?? throw new ArgumentNullException(nameof(messageRouteRegistry));
            _messageSerializer = messageSerializer ?? throw new ArgumentNullException(nameof(messageSerializer));

            _cancellationTokenSource = new CancellationTokenSource();

            _config.Value.ProducerConfig.ClientId = Dns.GetHostName();
            _config.Value.ConsumerConfig.ClientId = Dns.GetHostName();
        }

        /// <summary>
        /// Consume message from message broker
        /// <seealso cref="IMessageRouteRegistry"/>
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="ConsumeException" />
        public virtual Task ConsumeAsync<TMessage>(string sourceTopic, Action<TMessage> dispatcher, CancellationToken cancellationToken = default) 
            where TMessage : class
        {
            using(var consumer = new ConsumerBuilder<Null, byte[]>(_config.Value.ProducerConfig).Build())
            {
                consumer.Subscribe(sourceTopic);

                var consumeTask = Task.Factory.StartNew(() =>
                {
                    while (true)
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
                            }
                        }
                        catch (ConsumeException ex)
                        {
                            _logger.LogError(ex, $"{_config.Value.Name} consume failed for {typeof(TMessage).Name} :: {sourceTopic}");
                        }
                    }

                }, cancellationToken);

                return consumeTask;
            }
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
            var messageRoute = GetMessageRoute<TMessage>();

            using (var producer = new ProducerBuilder<Null, byte[]>(_config.Value.ProducerConfig).Build())
            {
                var payload = new Message<Null, byte[]>()
                {
                    Value = _messageSerializer.Serialize(message),
                    Timestamp = new Timestamp(DateTime.UtcNow, TimestampType.LogAppendTime)
                };

                return producer.ProduceAsync(messageRoute, payload, cancellationToken);
            }
        }

        /// <summary>
        /// Canacel all subscribtions
        /// </summary>
        public virtual void Unsubscribe()
        {
            _cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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
