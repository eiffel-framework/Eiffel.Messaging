using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Abstractions
{
    /// <summary>
    /// Sends messages to message broker
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// Send message
        /// </summary>
        void Send<TMessage>(TMessage message)
           where TMessage : class;

        /// <summary>
        /// Send message 
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        Task SendAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
            where TMessage : class;

        /// <summary>
        /// Subscribe topic which message registered on IMessageRouteRegistry
        /// </summary>
        void Subscribe<TMessage>()
            where TMessage : class;

        /// <summary>
        /// Subscribe topic which message registered on IMessageRouteRegistry
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        Task SubscribeAsync<TMessage>(CancellationToken cancellationToken = default)
            where TMessage : class;

        /// <summary>
        /// Unsubscribe topic
        /// </summary>
        void Unsubscribe();
    }
}
