using Eiffel.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Tests
{
    public class MockEventHandler : IEventHandler<MockEvent>
    {
        public Task HandleAsync(MockEvent payload, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
