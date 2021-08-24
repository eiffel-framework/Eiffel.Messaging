using Eiffel.Messaging.Abstractions;

namespace Eiffel.Messaging.Tests
{
    [Message(Route = "mock-route")]
    public class MockCommand : ICommand
    {
    }
}
