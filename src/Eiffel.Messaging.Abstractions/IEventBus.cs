using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Abstractions
{
    // TODO: where TEvent : DomainEvent

    /// <summary>
    /// Publish events to message broker or in memory stream
    /// </summary>
    /// <exception cref="ArgumentNullException" />
    public interface IEventBus
    {
        /// <summary>
        /// Publish event synchronically
        /// </summary>
        void Publish<TEvent>(TEvent @event)
            where TEvent : class;

        /// <summary>
        /// Publish event asynchronously
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : class;

        /// <summary>
        /// Subscribe event synchronically
        /// </summary>
        void Subscribe<TEvent>()
            where TEvent : class;

        /// <summary>
        /// Subscribe event synchronically
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        Task SubscribeAsync<TEvent>(CancellationToken cancellationToken = default)
            where TEvent : class;

        /// <summary>
        /// Unsubscribe event bus
        /// </summary>
        void Unsubscribe();
    }
}
