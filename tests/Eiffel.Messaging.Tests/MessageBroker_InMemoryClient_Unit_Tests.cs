using Eiffel.Messaging.Abstractions;
using Eiffel.Messaging.InMemory;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Eiffel.Messaging.Tests
{
    public class MessageBroker_InMemoryClient_Unit_Tests
    {
        private readonly IMessageBrokerClient _client;
        private readonly IMessageRouteRegistry _messageRouteRegistry;

        private const string Name = "Eiffel";
        private const int EventId = 1;

        public MessageBroker_InMemoryClient_Unit_Tests()
        {
            _messageRouteRegistry = new MessageRouteRegistry();

            _client = new InMemoryClient(new Mock<ILogger<InMemoryClient>>().Object, new InMemoryClientConfig(), _messageRouteRegistry, new DefaultMessageSerializer());

            _messageRouteRegistry.Register<MockEvent>("mock-event-route");

            _client.ProduceAsync(new MockEvent(EventId));
        }

        [Fact]
        public async Task Consume_Should_Throw_Exception_When_MessageRoute_IsMissing()
        {
            // Arrange
            MockMessage result = null;

            // Act
            Func<Task> sutConsume = () => _client.ConsumeAsync<MockMessage>((message) =>
            {
                result = message;
            }, default);

            // Assert
            await Assert.ThrowsAsync<MessageRouteNotFoundException>(sutConsume);
        }

        [Fact]
        public async Task Produce_Should_Throw_Exception_When_MessageRoute_IsMissing()
        {
            // Arrange
            var message = new MockMessage(Name);

            // Act
            Func<Task> sutConsume = () => _client.ProduceAsync(message);

            // Assert
            await Assert.ThrowsAsync<MessageRouteNotFoundException>(sutConsume);
        }

        [Fact]
        public async Task Consume_Shoud_Throw_Exception_When_Operation_Cancelled()
        {
            // Arrange
            var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));

            await Task.Delay(2000);

            // Arrange
            MockEvent result = null;

            // Act
            Func<Task> sutConsume = () =>_client.ConsumeAsync<MockEvent>((@event) =>
            {
                result = @event;
            }, tokenSource.Token);

            // Assert
            await Assert.ThrowsAsync<OperationCanceledException>(sutConsume);
        }

        [Fact]
        public async Task Produce_Should_Throw_Exception_When_Operation_Cancelled()
        {
            // Arrange
            var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));

            await Task.Delay(2000);

            // Act
            Func<Task> sutProduce = () => _client.ProduceAsync(new MockMessage(), tokenSource.Token);

            // Assert
            await Assert.ThrowsAsync<OperationCanceledException>(sutProduce);
        }

        [Fact]
        public async Task Client_Should_Produce_Message()
        {
            // Arrange
            var message = new MockMessage(Name);
            MockMessage result = null;

            _messageRouteRegistry.Register<MockMessage>("mock-meesage-route");

            await _client.ProduceAsync(message);

            // Act 
            await _client.ConsumeAsync<MockMessage>((message) =>
            {
                result = message;
            }, default);

            // Assert
            result.Should().NotBeNull();

            result.Name.Should().Be(Name);
        }

        [Fact]
        public async Task Client_Should_Consume_Message()
        {
            // Arrange
            MockEvent result = null;

            // Act
            await _client.ConsumeAsync<MockEvent>((@event) =>
            {
                result = @event;
            }, default);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(EventId);
        }
    }
}
