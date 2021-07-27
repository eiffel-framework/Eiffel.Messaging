using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Abstractions
{
    /// <summary>
    /// Mediator pipeline post processor
    /// </summary>
    public interface IPipelinePostProcessor
    {
        Task ProcesAsync(object message, CancellationToken cancellationToken = default);
    }
}
