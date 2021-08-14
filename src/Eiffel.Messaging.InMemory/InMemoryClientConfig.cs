using Eiffel.Messaging.Abstractions;
using Eiffel.Messaging.Exceptions;

namespace Eiffel.Messaging.InMemory
{
    public class InMemoryClientConfig : IMessageBrokerClientConfig
    {
        public string Name => "InMemory";

        public int Capacity { get; set; } = 128;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new InvalidConfigurationException("Configuration name must be specified");
            }
        }
    }
}
