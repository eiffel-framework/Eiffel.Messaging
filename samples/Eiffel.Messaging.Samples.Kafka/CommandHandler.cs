using Eiffel.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Samples.Kafka
{
    public class CreateOrder : ICommand
    {
        public string UserId { get; set; }
    }

    public class CommandHandler : ICommandHandler<CreateOrder>
    {
        public Task HandleAsync(CreateOrder payload, CancellationToken cancellationToken = default)
        {
            
        }
    }
}
