using Eiffel.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Tests
{
    public class MockCommandHandler : ICommandHandler<MockCommand>
    {
        public Task HandleAsync(MockCommand payload, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
