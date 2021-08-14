using Eiffel.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Tests
{
    public class MockAuditLoggingPipeline : IPipelinePostProcessor
    {
        public Task ProcessAsync(object message, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
