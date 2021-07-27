using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Abstractions
{
    /// <summary>
    /// Messaage broker client
    /// </summary>
    public interface IMessageBrokerClient
    {
        /// <summary>
        /// Sends message to message broker
        /// <seealso cref="IMessageRouteRegistry"/>
        /// </summary>
        /// <typeparam name="TMessage">Message object</typeparam>
        void Produce<TMessage>(TMessage message)
            where TMessage : class, new();

        /// <summary>
        /// Sends message to message broker
        /// <seealso cref="IMessageRouteRegistry"/>
        /// </summary>
        /// <typeparam name="TMessage">Message object</typeparam>
        /// <exception cref="OperationCanceledException" />
        Task ProduceAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
            where TMessage : class, new();

        /// <summary>
        /// Comsumes message from message broker
        /// <seealso cref="IMessageRouteRegistry"/>
        /// </summary>
        /// <typeparam name="TMessage">Message object</typeparam>
        void Consume<TMessage>(Action<TMessage> dispatcher)
            where TMessage : class, new();

        /// <summary>
        /// Sends message to message broker
        /// <seealso cref="IMessageRouteRegistry"/>
        /// </summary>
        /// <typeparam name="TMessage">Message object</typeparam>
        /// <exception cref="OperationCanceledException" />
        Task ConsumeAsync<TMessage>(Action<TMessage> dispatcher, CancellationToken cancellationToken = default)
            where TMessage : class, new();

        /// <summary>
        /// Unsubscribe
        /// </summary>
        void Unsubscribe();
    }
}
