using Eiffel.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Tests
{
    public class MockMessageHandler : IMessageHandler<MockMessage>
    {
        public Task HandleAsync(MockMessage payload, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
