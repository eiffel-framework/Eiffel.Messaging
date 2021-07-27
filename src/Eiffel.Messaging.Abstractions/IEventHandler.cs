using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Abstractions
{
    public interface IEventHandler<in TPayload>
       where TPayload : IEvent
    {
        /// <summary>
        /// Handle message
        /// </summary>
        Task HandleAsync(TPayload payload, CancellationToken cancellationToken = default);
    }
}
