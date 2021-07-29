using System;
using System.Threading;
using System.Threading.Tasks;
using Eiffel.Messaging.Abstractions;

namespace Eiffel.Messaging
{
    /// <summary>
    /// Publish events to message broker or in memory stream
    /// </summary>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    public class EventBus : IEventBus
    {
        private readonly IMediator _mediator;
        private readonly IMessageBrokerClient _client;

        public EventBus(IMediator mediator, IMessageBrokerClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Publish event asynchronously
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        public virtual Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) 
            where TEvent : class
        {
            return _client.ProduceAsync(@event, cancellationToken);
        }

        /// <summary>
        /// Subscribe event synchronically
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        public virtual Task SubscribeAsync<TEvent>(CancellationToken cancellationToken = default) 
            where TEvent : class
        {
            return _client.ConsumeAsync(async (TEvent @event) =>
            {
                await _mediator.PublishAsync(@event, cancellationToken);
            }, cancellationToken);
        }

        /// <summary>
        /// Unsubscribe event bus
        /// </summary>
        public virtual void Unsubscribe()
        {
            _client.Unsubscribe();
        }
    }
}
