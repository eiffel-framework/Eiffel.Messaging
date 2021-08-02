using Autofac;
using Eiffel.Messaging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Publish events to handler(s)
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="HandlerNotFoundException" />
        /// <exception cref="MissingMethodException" />
        public virtual Task PublishAsync<TEvent>(TEvent payload, CancellationToken cancellationToken = default)
            where TEvent : class
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            var handlerType = typeof(IEnumerable<>).MakeGenericType(typeof(IEventHandler<>).MakeGenericType(payload.GetType()));

            var handlers = _lifetimeScope.Resolve(handlerType) as dynamic;

            if (handlers == null || handlers?.Length == 0)
            {
                throw new HandlerNotFoundException($"{payload.GetType().Name} event handler could not be found");
            }

            var tasks = new List<Task>();

            foreach (var handler in handlers)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                tasks.Add(handler?.HandleAsync(payload, cancellationToken));
            }

            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Dispatch message to handler
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="HandlerNotFoundException" />
        /// <exception cref="MissingMethodException" />
        public virtual Task DispatchAsync<TMessage>(TMessage payload, CancellationToken cancellationToken = default) where TMessage : class
        {
            var handlerType = typeof(IMessageHandler<>).MakeGenericType(payload.GetType());

            return HandleAsync<TMessage, Task>(payload, handlerType, cancellationToken);
        }

        /// <summary>
        /// Dispatch query to handler
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="HandlerNotFoundException" />
        /// <exception cref="MissingMethodException" />
        public virtual Task<TReply> SendAsync<TReply>(IQuery<TReply> payload, CancellationToken cancellationToken = default) 
            where TReply : class
        {
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(payload.GetType(), typeof(TReply));

            return HandleAsync<IQuery<TReply>, Task<TReply>>(payload, handlerType, cancellationToken);
        }

        /// <summary>
        /// Dispatch command to handler
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="HandlerNotFoundException" />
        /// <exception cref="MissingMethodException" />
        public virtual Task<TResult> SendAsync<TResult>(ICommand payload, CancellationToken cancellationToken = default) where TResult : IEquatable<TResult>
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(payload.GetType());

            return HandleAsync<ICommand, Task<TResult>>(payload, handlerType, cancellationToken);
        }

        /// <summary>
        /// Dispatch command to handler
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="HandlerNotFoundException" />
        /// <exception cref="MissingMethodException" />
        public virtual Task SendAsync(ICommand payload, CancellationToken cancellationToken = default)
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(payload.GetType());

            return HandleAsync<ICommand, Task>(payload, handlerType, cancellationToken);
        }

        private TResult HandleAsync<TPayload, TResult>(TPayload payload, Type handlerType, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            if (!_lifetimeScope.IsRegistered(handlerType))
            {
                throw new HandlerNotFoundException(handlerType.Name);
            }

            var targetMethod = handlerType.GetMethod("HandleAsync", new[] { payload.GetType(), typeof(CancellationToken) });

            if (targetMethod == null)
            {
                throw new MissingMethodException("Handler method is missing");
            }

            var handler = _lifetimeScope.Resolve(handlerType);

            _lifetimeScope.Resolve<IEnumerable<IPipelinePreProcessor>>()?.ToList()?
                .ForEach(async pre => await pre.ProcessAsync(payload).ConfigureAwait(false));

            var result = (TResult)targetMethod.Invoke(handler, new object[] { payload, cancellationToken });

            _lifetimeScope.Resolve<IEnumerable<IPipelinePostProcessor>>()?.ToList()?
                .ForEach(async post => await post.ProcessAsync(payload, cancellationToken).ConfigureAwait(false));

            return result;
        }
    }
}
