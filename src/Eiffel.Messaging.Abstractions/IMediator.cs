using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Abstractions
{
    public interface IMediator
    {
        // TODO: where TEvent : DomainEvent

        /// <summary>
        /// Publish event to registered event handler
        /// </summary>
        /// <exception cref="OperationCanceledException"
        Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
           where TEvent : class;
    }
}
