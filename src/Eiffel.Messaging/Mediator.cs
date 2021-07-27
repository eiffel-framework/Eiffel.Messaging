using Autofac;
using Eiffel.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging
{
    public class Mediator : IMediator
    {
        private readonly ILifetimeScope _lifetimeScope;

        public Mediator(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
        }

        public Task DispatchAsync<TMessage>(TMessage payload, CancellationToken cancellationToken = default) where TMessage : IMessage
        {
            throw new NotImplementedException();
        }

        public Task PublishAsync<TEvent>(TEvent payload, CancellationToken cancellationToken = default) where TEvent : class
        {
            throw new NotImplementedException();
        }

        public Task<TReply> RequestAsync<TReply>(IQuery<TReply> payload, CancellationToken cancellationToken = default) where TReply : class
        {
            throw new NotImplementedException();
        }

        public Task<TResult> SendAsync<TResult>(ICommand payload, CancellationToken cancellationToken = default) where TResult : IEquatable<TResult>
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(payload.GetType());

            if (!_lifetimeScope.IsRegistered(handlerType))
            {
                throw new HandlerNotFoundException(handlerType.Name);
            }

            var handler = _lifetimeScope.Resolve(handlerType) as dynamic;

            return handler.HandleAsync(payload, cancellationToken);
        }
    }
}
