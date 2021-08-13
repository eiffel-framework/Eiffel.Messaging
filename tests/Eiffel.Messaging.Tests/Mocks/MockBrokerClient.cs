using Eiffel.Messaging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eiffel.Messaging.Tests
{
    public class MockBrokerClient : IMessageBrokerClient
    {
        public MockBrokerClient(
            Logger<MockBrokerClient> logger, 
            MockBrokerClientConfig config,
            IMessageRouteRegistry messageRouteRegistry,
            IMessageSerializer messageSerializer)
        {
        }


        public Task ConsumeAsync<TMessage>(string sourceTopic, Action<TMessage> dispatcher, CancellationToken cancellationToken = default) where TMessage : class
        {
            throw new NotImplementedException();
        }

        public Task ProduceAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe()
        {
            throw new NotImplementedException();
        }
    }
}
