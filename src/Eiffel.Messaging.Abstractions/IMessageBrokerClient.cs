﻿using System;
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
        /// <seealso cref="IMessageRegistry"/>
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        Task ProduceAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
            where TMessage : class;

        /// <summary>
        /// Consume message from message broker
        /// <seealso cref="IMessageRegistry"/>
        /// </summary>
        /// <exception cref="OperationCanceledException" />
        Task ConsumeAsync<TMessage>(Action<TMessage> dispatcher, CancellationToken cancellationToken = default)
            where TMessage : class;

        /// <summary>
        /// Unsubscribe
        /// </summary>
        void Unsubscribe();
    }
}
