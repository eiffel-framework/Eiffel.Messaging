using Eiffel.Messaging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.InMemory
{
    public class InMemoryClient : IMessageBrokerClient, IDisposable
    {
        private readonly ConcurrentDictionary<string, BehaviorSubject<byte[]>> _subscriptions;
        private readonly InMemoryClientConfig _config;
        private readonly IMessageRouteRegistry _messageRouteRegistry;
        private readonly IMessageSerializer _messageSerializer;

        public InMemoryClient(InMemoryClientConfig config, IMessageRouteRegistry messageRouteRegistry, IMessageSerializer messageSerializer)
        {
            _subscriptions = new ConcurrentDictionary<string, BehaviorSubject<byte[]>>();
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

            var topicName = _messageRouteRegistry.GetRoute<TMessage>();

            CreateSubscription(topicName);

            _subscriptions[topicName].Subscribe((bytes) =>
            {
                if (bytes == null) return;

                var message = _messageSerializer.Deserialize<TMessage>(bytes);

                dispatcher.Invoke(message);
            });

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


            var topicName = _messageRouteRegistry.GetRoute<TMessage>(); 

            var bytes = _messageSerializer.Serialize(message);

            CreateSubscription(topicName);

            _subscriptions[topicName].OnNext(bytes);
            
            return Task.CompletedTask;
        }

        public virtual void Unsubscribe()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Value.Dispose();
            }
            _subscriptions.Clear();
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

        private void CreateSubscription(string topicName)
        {
            if (!_subscriptions.ContainsKey(topicName))
            {
                var result = _subscriptions.TryAdd(topicName, new BehaviorSubject<byte[]>(null));
                if (!result)
                    throw new AccessViolationException();
            }
        }
    }
}
