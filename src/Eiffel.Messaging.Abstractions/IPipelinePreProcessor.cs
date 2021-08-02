using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Abstractions
{
    /// <summary>
    /// Mediator pipeline pre processor
    /// </summary>
    public interface IPipelinePreProcessor
    {
        Task ProcessAsync(object message, CancellationToken cancellationToken = default);
    }
}
