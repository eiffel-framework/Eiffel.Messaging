using Eiffel.Messaging.Abstractions;

namespace Eiffel.Messaging.Tests
{
    [Message("mock-message-route")]
    public class MockMessage : IMessage
    {
        public MockMessage()
        {

        }
        public MockMessage(string name)
        {
            Name = name;
        }
        public string Name { get; }
    }
}
