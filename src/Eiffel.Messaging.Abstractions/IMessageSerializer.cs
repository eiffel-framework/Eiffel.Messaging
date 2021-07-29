namespace Eiffel.Messaging.Abstractions
{
    public interface IMessageSerializer
    {
        byte[] Serialize<TMessage>(TMessage message);

        TMessage Deserialize<TMessage>(byte[] bytes);
    }

    public interface IBinarySerializer
    {
        byte[] Serialize<TMessage>(TMessage message)
            where TMessage : new();

        TMessage Deserialize<TMessage>(byte[] bytes)
            where TMessage : new();
    }
}
