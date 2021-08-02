using Eiffel.Messaging.Abstractions;
using Utf8Json;

namespace Eiffel.Messaging
{
    public class DefaultMessageSerializer : IMessageSerializer
    {
        public TMessage Deserialize<TMessage>(byte[] bytes)
        {
            return JsonSerializer.Deserialize<TMessage>(bytes);
        }

        public byte[] Serialize<TMessage>(TMessage message)
        {
            return JsonSerializer.Serialize<TMessage>(message);
        }
    }
}
