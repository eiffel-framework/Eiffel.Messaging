namespace Eiffel.Messaging.Abstractions
{
    /// <summary>
    /// Message broker client config
    /// </summary>
    public interface IMessageBrokerClientConfig
    {
        public string Name { get; }
        bool IsEnabled { get; set; }
    }
}
