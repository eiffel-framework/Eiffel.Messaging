namespace Eiffel.Messaging.Abstractions
{
    public interface IMessageSerializer
    {
        byte[] Serialize<TMessage>(TMessage message);

        TMessage Deserialize<TMessage>(byte[] bytes);
    }
}
