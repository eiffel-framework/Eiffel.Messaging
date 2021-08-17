using Eiffel.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Samples.InMemory
{
    [MessageRoute("sample-route")]
    public class Notification : IMessage
    {
        public Notification(string message)
        {
            Message = message;
        }
        public string Message { get; }
    }

    public class MessageHandler : IMessageHandler<Notification>
    {
        public async Task HandleAsync(Notification payload, CancellationToken cancellationToken = default)
        {
            await Console.Out.WriteLineAsync(payload.Message);
        }
    }

   
}
