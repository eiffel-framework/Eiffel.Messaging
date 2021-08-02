using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Abstractions
{
    /// <summary>
    /// Handle commands, TRetVal must be primitive type if needed
    /// <seealso cref="CommandHandler{TPayload}"/>
    /// <typeparam name="TPayload">A command object</typeparam>
    /// <typeparamref name="TRetVal">A primitive type</typeparamref>
    /// </summary>
    public interface ICommandHandler<TPayload, TRetVal>
        where TPayload : ICommand
        where TRetVal : IEquatable<TRetVal>
    {
        Task<TRetVal> HandleAsync(TPayload payload, CancellationToken cancellationToken = default);
    }


    /// <summary>
    /// Handle commands
    /// </summary>
    /// <typeparam name="TPayload">A command object</typeparam>
    public interface ICommandHandler<in TPayload>
        where TPayload : ICommand
    {
        Task HandleAsync(TPayload payload, CancellationToken cancellationToken = default);
    }
}
