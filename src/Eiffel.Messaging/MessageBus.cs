using System;
using System.Threading;
using System.Threading.Tasks;
using Eiffel.Messaging.Abstractions;

namespace Eiffel.Messaging
{
    public class MessageBus : IMessageBus
    {
        private readonly IMediator _mediator;
        private readonly IMessageBrokerClient _client;

        public MessageBus(IMediator mediator, IMessageBrokerClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Send message 
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        public async Task SendAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) 
            where TMessage : class
        {
            await _client.ProduceAsync(message, cancellationToken);
        }

        /// <summary>
        /// Subscribe message
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        public Task SubscribeAsync<TMessage>(CancellationToken cancellationToken = default) 
            where TMessage : class
        {
            return _client.ConsumeAsync<TMessage>(async (message) =>
            {
                if (message is ICommand cmd)
                    await _mediator.SendAsync(cmd, cancellationToken);
                else if (message is IMessage msg)
                    await _mediator.DispatchAsync(msg, cancellationToken);
                else
                    throw new ArgumentException($"{ nameof(TMessage) } must be implemented from ICommand or IMessage");
            }, cancellationToken);
        }

        /// <summary>
        /// Unsubscribe topic
        /// </summary>
        public void Unsubscribe()
        {
            _client.Unsubscribe();
        }
    }
}
