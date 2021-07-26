﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Abstractions
{
    /// <summary>
    /// Handles commands, TRetVal must be primitive type if needed
    /// <seealso cref="CommandHandler{TPayload}"/>
    /// <typeparam name="TPayload">A command object</typeparam>
    /// <typeparamref name="TRetVal">A primitive type</typeparamref>
    /// </summary>
    public interface ICommandHandler<TPayload, TRetVal>
        where TPayload : ICommand
        where TRetVal : IEquatable<TRetVal>
    {
        public abstract Task<TRetVal> HandleAsync(TPayload payload, CancellationToken cancellationToken);
    }


    /// <summary>
    /// Handles commands
    /// </summary>
    /// <typeparam name="TPayload">A command object</typeparam>
    public interface ICommandHandler<in TPayload>
        where TPayload : ICommand
    {
        public abstract Task HandleAsync(TPayload payload, CancellationToken cancellationToken);
    }
}
