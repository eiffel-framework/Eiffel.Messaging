using BinaryPack;
using Eiffel.Messaging.Abstractions;

namespace Eiffel.Messaging
{
    public class DefaultBinarySerializer : IBinarySerializer
    {
        public TMessage Deserialize<TMessage>(byte[] bytes)
            where TMessage : new()
        {
            return BinaryConverter.Deserialize<TMessage>(bytes);
        }

        public byte[] Serialize<TMessage>(TMessage message)
            where TMessage : new()
        {
            return BinaryConverter.Serialize(message);
        }
    }
}
