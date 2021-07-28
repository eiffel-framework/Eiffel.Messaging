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
        Task PublishAsync<TEvent>(TEvent payload, CancellationToken cancellationToken = default)
            where TEvent : IEvent;

        /// <summary>
        /// Dispatch message to handler
        /// </summary>
        Task DispatchAsync<TMessage>(TMessage payload, CancellationToken cancellationToken = default)
            where TMessage : class;

        /// <summary>
        /// Dispatch query and returns response from handler
        /// </summary>
        /// <typeparam name="TReply">Response object</typeparam>
        Task<TReply> SendAsync<TReply>(IQuery<TReply> payload, CancellationToken cancellationToken = default)
            where TReply : class;

        /// <summary>
        /// Dispatch command to handler. According to use case return command result (primitive types only)
        /// </summary>
        Task<TResult> SendAsync<TResult>(ICommand payload, CancellationToken cancellationToken = default)
            where TResult : IEquatable<TResult>;

        /// <summary>
        /// Dispatch command to handler
        /// </summary>
        Task SendAsync(ICommand payload, CancellationToken cancellationToken = default);
    }
}
