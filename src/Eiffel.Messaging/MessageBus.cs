﻿using System;
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
        public void Send<TMessage>(TMessage message) 
            where TMessage : class
        {
            _client.Produce(message);
        }

        /// <summary>
        /// Send message 
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        public Task SendAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) 
            where TMessage : class
        {
            return _client.ProduceAsync(message, cancellationToken);
        }

        /// <summary>
        /// Subscribe message
        /// </summary>
        public void Subscribe<TMessage>()
            where TMessage : class
        {
            _client.Consume<TMessage>(async (message) =>
            {
                await _mediator.DispatchAsync(message, default);
            });
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
                await _mediator.DispatchAsync(message, cancellationToken);
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
