using Eiffel.Messaging.Abstractions;
using Eiffel.Messaging.Exceptions;

namespace Eiffel.Messaging.InMemory
{
    public class InMemoryClientConfig : IMessageBrokerClientConfig
    {
        public string Name => "InMemory";

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new InvalidConfigurationException("Configuration name must be specified");
            }
        }
    }
}
