using Eiffel.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Tests
{
    public class MockQueryHandler : IQueryHandler<MockQuery, MockQueryResponse>
    {
        public Task<MockQueryResponse> HandleAsync(MockQuery payload, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
