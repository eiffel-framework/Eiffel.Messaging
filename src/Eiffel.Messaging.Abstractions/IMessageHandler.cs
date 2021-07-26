using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Abstractions
{
    public interface IMessageHandler<in TPayload>
        where TPayload : IMessage
    {
        Task HandleAsync(TPayload payload, CancellationToken cancellationToken = default);
    }
}
