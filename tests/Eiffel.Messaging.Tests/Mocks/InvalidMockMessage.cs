using Eiffel.Messaging.Abstractions;

namespace Eiffel.Messaging.Tests.Mocks
{
    [Message(Route = "")]
    public class InvalidMockMessage : IMessage
    {
    }
}
