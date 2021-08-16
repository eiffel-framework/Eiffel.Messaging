using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using Eiffel.Messaging.Abstractions;

namespace Eiffel.Messaging.Samples.Kafka
{
    public class Worker : BackgroundService
    {
        private readonly IMessageBus _messageBus;

        public Worker(IMessageBus messageBus)
        {
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    await _messageBus.SendAsync(new CreateOrder() { UserId = Guid.NewGuid().ToString() });
                }
            }).ConfigureAwait(false);
        }
    }
}
