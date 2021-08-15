using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using Eiffel.Messaging.Abstractions;

namespace Eiffel.Messaging
{
    public class ConsumerService<TMessage> : BackgroundService
        where TMessage : class
    {
        private readonly IMessageBus _messageBus;

        public ConsumerService(IMessageBus messageBus)
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

                    await _messageBus.SubscribeAsync<TMessage>(stoppingToken);
                }
            }).ConfigureAwait(false);
        }
    }
}
