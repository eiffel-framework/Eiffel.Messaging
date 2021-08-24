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
        private readonly IEventBus _eventBus;

        public ConsumerService(IMessageBus messageBus, IEventBus eventBus)
        {
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (typeof(TMessage).IsAssignableTo(typeof(IEvent)))
                    {
                        await _eventBus.SubscribeAsync<TMessage>(stoppingToken);
                    }
                    else
                    {
                        await _messageBus.SubscribeAsync<TMessage>(stoppingToken);
                    }
                }
            });
        }
    }
}
