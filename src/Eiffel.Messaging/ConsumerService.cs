using Eiffel.Messaging.Abstractions;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging
{
    public class ConsumerService : IHostedService
    {
        private readonly IMessageRouteRegistry _messageRouteRegistry;
        private readonly IMessageBus _messageBus;

        public ConsumerService(IMessageRouteRegistry messageRouteRegistry, IMessageBus messageBus)
        {
            _messageRouteRegistry = messageRouteRegistry ?? throw new ArgumentNullException(nameof(messageRouteRegistry));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var tasks = CreateConsumerTasks(cancellationToken);

            await Task.WhenAll(tasks);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _messageBus.Unsubscribe();

            return Task.CompletedTask;
        }

        private IEnumerable<Task> CreateConsumerTasks(CancellationToken cancellationToken)
        {
            foreach(var route in _messageRouteRegistry.Routes)
            {
                var method = _messageBus.GetType().GetMethod("SubscribeAsync").MakeGenericMethod(route.Item1);
                yield return (Task)method.Invoke(_messageBus, new object[] { cancellationToken });
            }
        }
    }
}
