using Confluent.Kafka;
using Eiffel.Messaging.Abstractions;
using Eiffel.Messaging.Exceptions;

namespace Eiffel.Messaging.Kafka
{
    public class KafkaClientConfig : IMessageBrokerClientConfig
    {
        public string Name => "Kafka";
        
        public ConsumerConfig ConsumerConfig { get; set; }
        
        public ProducerConfig ProducerConfig { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new InvalidConfigurationException("Configuration name must be specified.");
            }

            if (ConsumerConfig == null)
            {
                throw new InvalidConfigurationException("ConsumerConfig is missing.");
            }

            if (ProducerConfig == null)
            {
                throw new InvalidConfigurationException("ProducerConfig is missing.");
            }
        }
    }
}
