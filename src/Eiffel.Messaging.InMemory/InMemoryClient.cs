using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Eiffel.Messaging.Abstractions;

namespace Eiffel.Messaging.InMemory
{
    public class InMemoryClient : IMessageBrokerClient, IDisposable
    {
        private readonly ConcurrentDictionary<string, BlockingCollection<byte[]>> _messageQueue;

        private readonly ILogger<InMemoryClient> _logger;
        private readonly InMemoryClientConfig _config;

        private readonly IMessageRouteRegistry _messageRouteRegistry;
        private readonly IMessageSerializer _messageSerializer;

        public InMemoryClient(
            ILogger<InMemoryClient> logger, 
            InMemoryClientConfig config, 
            IMessageRouteRegistry messageRouteRegistry, 
            IMessageSerializer messageSerializer)
        {
            _messageQueue = new ConcurrentDictionary<string, BlockingCollection<byte[]>>();

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _messageRouteRegistry = messageRouteRegistry ?? throw new ArgumentNullException(nameof(messageRouteRegistry));
            _messageSerializer = messageSerializer ?? throw new ArgumentNullException(nameof(messageSerializer));
        }

        /// <summary>
        /// Consume message from message broker
        /// <seealso cref="IMessageRouteRegistry"/>
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        public virtual Task ConsumeAsync<TMessage>(Action<TMessage> dispatcher, CancellationToken cancellationToken = default)
            where TMessage : class
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            var targetTopic = _messageRouteRegistry.GetRoute<TMessage>();

            if (!_messageQueue.ContainsKey(targetTopic))
            {
                return Task.CompletedTask;
            }

            _messageQueue[targetTopic].TryTake(out byte[] bytes, 0, cancellationToken);

            if (bytes == null)
            {
                return Task.CompletedTask;
            }

            var message = _messageSerializer.Deserialize<TMessage>(bytes);

            dispatcher.Invoke(message);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sends message to message broker
        /// <seealso cref="IMessageRouteRegistry"/>
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        public virtual Task ProduceAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
            where TMessage : class
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            var targetTopic = _messageRouteRegistry.GetRoute<TMessage>();

            if (!_messageQueue.ContainsKey(targetTopic))
            {
                _messageQueue.TryAdd(targetTopic, new BlockingCollection<byte[]>(_config.Capacity));
            }

            var bytes = _messageSerializer.Serialize(message);

            _messageQueue[targetTopic].TryAdd(bytes, 0, cancellationToken);

            return Task.CompletedTask;
        }

        public virtual void Unsubscribe()
        {
            foreach(var topic in _messageQueue.Keys)
            {
                _messageQueue[topic].CompleteAdding();
                _messageQueue[topic].Dispose();
            }
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
            Unsubscribe();
        }
    }
}
