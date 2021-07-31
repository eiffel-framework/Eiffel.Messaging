using Confluent.Kafka;
using Eiffel.Messaging.Abstractions;

namespace Eiffel.Messaging.Kafka
{
    public class KafkaClientConfig : IMessageBrokerClientConfig
    {
        public string Name => "Kafka";
        
        public ConsumerConfig ConsumerConfig { get; set; }
        
        public ProducerConfig ProducerConfig { get; set; }
    }
}
