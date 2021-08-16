using Eiffel.Messaging.Abstractions;
using Eiffel.Messaging.Exceptions;

namespace Eiffel.Messaging.Tests
{
    public class MockBrokerClientConfig : IMessageBrokerClientConfig
    {
        public string Name => "Mock";

        public int? Count { get; set; }

        public void Validate()
        {
            if (!Count.HasValue)
            {
                throw new InvalidConfigurationException($"{nameof(Count)} must be specified.");
            }
        }
    }
}
