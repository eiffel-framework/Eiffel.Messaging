using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Abstractions
{
    /// <summary>
    /// Mediator pipeline pre processor
    /// </summary>
    public interface IPipelinePreProcessor
    {
        Task ProcesAsync(object message, CancellationToken cancellationToken = default);
    }
}
