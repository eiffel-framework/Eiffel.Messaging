using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Abstractions
{
    /// <summary>
    /// Handle queries
    /// </summary>
    /// <typeparam name="TPayload">Query object</typeparam>
    /// <typeparam name="TResult">Response class</typeparam>
    public interface IQueryHandler<TPayload, TResult>
    {
        public abstract Task<TResult> HandleAsync(TPayload payload, CancellationToken cancellationToken = default);
    }
}
