using Eiffel.Messaging.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Samples.InMemory
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMessageBus _messageBus;

        public Worker(ILogger<Worker> logger, IMessageBus messageBus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int i = 1;
            while (!stoppingToken.IsCancellationRequested)
            {
                await _messageBus.SendAsync(new Notification($"Hello World {i} !")).ConfigureAwait(false);
                i++;
            }
        }
    }
}
