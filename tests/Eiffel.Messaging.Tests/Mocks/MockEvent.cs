using Eiffel.Messaging.Abstractions;

namespace Eiffel.Messaging.Tests
{
    [Message(Route = "mock-event")]
    public class MockEvent : IEvent
    {
        public MockEvent()
        {

        }

        public MockEvent(int id)
        {
            Id = id;
        }

        public int Id { get; private set; }
    }
}
