using Eiffel.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Samples.InMemory
{
    [Message(Route = "sample-route")]
    public class Hello : IMessage
    {
        public Hello(string message)
        {
            Message = message;
        }
        public string Message { get; }
    }

    public class MessageHandler : IMessageHandler<Hello>
    {
        public async Task HandleAsync(Hello payload, CancellationToken cancellationToken = default)
        {
            await Console.Out.WriteLineAsync(payload.Message);
        }
    }
   
}
