using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Abstractions
{
    /// <summary>
    /// Handle messages
    /// </summary>
    /// <typeparam name="TPayload">Message object</typeparam>
    public interface IMessageHandler<in TPayload>
        where TPayload : IMessage
    {
        /// <summary>
        /// Handle message
        /// </summary>
        Task HandleAsync(TPayload payload, CancellationToken cancellationToken = default);
    }
}
