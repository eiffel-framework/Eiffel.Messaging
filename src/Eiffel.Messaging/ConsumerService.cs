using Autofac;
using Eiffel.Messaging.Abstractions;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging
{
    public class ConsumerService : BackgroundService
    {
        private readonly IMessageRouteRegistry _messageRouteRegistry;
        private readonly IMessageBus _messageBus;

        public ConsumerService(IMessageRouteRegistry messageRouteRegistry, IMessageBus messageBus)
        {
            _messageRouteRegistry = messageRouteRegistry ?? throw new ArgumentNullException(nameof(messageRouteRegistry));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tasks = CreateConsumerTasks(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.WhenAll(tasks);
            }
        }

        private IEnumerable<Task> CreateConsumerTasks(CancellationToken cancellationToken)
        {
            foreach (var route in _messageRouteRegistry.Routes)
            {
                var method = _messageBus.GetType().GetMethod("SubscribeAsync").MakeGenericMethod(route.Item1);

                yield return Task.Run(async () =>
                {
                    await (Task)method.Invoke(_messageBus, new object[] { cancellationToken });
                });
            }
        }
    }
}
