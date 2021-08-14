using Eiffel.Messaging.Abstractions;

namespace Eiffel.Messaging.Tests
{
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
