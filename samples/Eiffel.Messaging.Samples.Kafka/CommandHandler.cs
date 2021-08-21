using Eiffel.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Samples.Kafka
{
    [Message("ORDERS")]
    public class CreateOrder : ICommand
    {
        public string UserId { get; set; }
    }

    public class CommandHandler : ICommandHandler<CreateOrder>
    {
        public async Task HandleAsync(CreateOrder payload, CancellationToken cancellationToken = default)
        {
            await System.Console.Out.WriteLineAsync(payload.UserId);
        }
    }
}
