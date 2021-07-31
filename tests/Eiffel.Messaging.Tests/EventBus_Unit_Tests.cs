﻿using Moq;
using System;
using System.Threading.Tasks;
using Eiffel.Messaging.Abstractions;
using System.Threading;
using Xunit;

namespace Eiffel.Messaging.Tests
{

    public class EventBus_Unit_Tests
    {
        private readonly MessageBus _messageBus;
        private readonly Mock<IMessageBrokerClient> _mockClient;
        private readonly Mock<IMediator> _mockMediator;

        public EventBus_Unit_Tests()
        {
            _mockClient = new Mock<IMessageBrokerClient>();

            _mockMediator = new Mock<IMediator>();
            _mockMediator.Setup(x => x.DispatchAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()));

            _messageBus = new MessageBus(_mockMediator.Object, _mockClient.Object);
        }

        [Fact]
        public async Task EventBus_Should_Send_Message()
        {
            // Arrange
            var mockMessage = new MockMessage();
            _mockClient.Setup(x => x.ProduceAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()));

            // Act
            await _messageBus.SendAsync(mockMessage);

            // Assert
            _mockClient.Verify(x => x.ProduceAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task EventBus_Should_Consume_Message()
        {
            // Arrange
            _mockClient.Setup(x => x.ConsumeAsync(It.IsAny<string>(), It.IsAny<Action<MockMessage>>(), It.IsAny<CancellationToken>()));

            // Act
            await _messageBus.SubscribeAsync<MockMessage>(It.IsAny<string>());

            // Assert
            _mockClient.Verify(x => x.ConsumeAsync(It.IsAny<string>(), It.IsAny<Action<MockMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
