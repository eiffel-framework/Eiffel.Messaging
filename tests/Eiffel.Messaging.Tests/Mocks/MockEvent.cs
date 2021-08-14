using Eiffel.Messaging.Abstractions;

namespace Eiffel.Messaging.Tests
{
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
