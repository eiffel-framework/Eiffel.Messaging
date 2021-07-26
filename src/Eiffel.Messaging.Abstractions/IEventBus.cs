using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Abstractions
{
    // TODO: where TEvent : DomainEvent

    /// <summary>
    /// Publish events to message broker or in memory stream
    /// </summary>
    /// <exception cref="ArgumentNullException">Mediator is null</exception>
    /// <exception cref="ArgumentNullException">MessageBrokerClient is null</exception>
    public interface IEventBus
    {
        /// <summary>
        /// Publish event synchronically
        /// </summary>
        /// <typeparam name="TEvent">Domain event</typeparam>
        void Publish<TEvent>(TEvent @event) 
            where TEvent : class, new();

        /// <summary>
        /// Publish event asynchronously
        /// </summary>
        /// <typeparam name="TEvent">Domain event</typeparam>
        /// <param name="event"></param>
        /// <exception cref="OperationCanceledException" />
        Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
            where TEvent : class, new();

        /// <summary>
        /// Subscribe event synchronically
        /// </summary>
        /// <typeparam name="TEvent">Domain event</typeparam>
        void Subscribe<TEvent>()
            where TEvent : class, new();

        /// <summary>
        /// Subscribe event synchronically
        /// </summary>
        /// <typeparam name="TEvent">Domain event</typeparam>
        /// <exception cref="OperationCanceledException" />
        Task SubscribeAsync<TEvent>(CancellationToken cancellationToken)
            where TEvent : class, new();
    }
}
