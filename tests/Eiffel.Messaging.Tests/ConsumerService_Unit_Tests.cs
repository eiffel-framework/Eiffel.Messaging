using Eiffel.Messaging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Eiffel.Messaging.Tests
{
    public class ConsumerService_Unit_Tests
    {
        private readonly Mock<IMessageBus> _mockMessageBus;
        private readonly Mock<IMessageRouteRegistry> _mockRegistry;

        private readonly ConsumerService _consumerService;

        public ConsumerService_Unit_Tests()
        {
            _mockMessageBus = new Mock<IMessageBus>();
            _mockRegistry = new Mock<IMessageRouteRegistry>();

            _consumerService = new ConsumerService(_mockRegistry.Object, _mockMessageBus.Object);

        }

        [Fact]
        public async Task ConsumerService_Should_ConsumeMessages_When_Service_Started()
        {
            // Arrange
            var messageRoutes = new HashSet<Tuple<Type, string>>()
            {
                new Tuple<Type, string>(typeof(MockMessage), "route1"),
            };

            _mockRegistry.SetupGet(x => x.Routes).Returns(messageRoutes);

            _mockMessageBus.Setup(x => x.SubscribeAsync<MockMessage>(It.IsAny<CancellationToken>()));

            // Act
            await _consumerService.StartAsync(default);

            await Task.Delay(3000);

            // Assert
            _mockMessageBus.Verify(x => x.SubscribeAsync<MockMessage>(It.IsAny<CancellationToken>()), Times.AtLeastOnce);

            _mockRegistry.VerifyGet(x => x.Routes, Times.AtLeastOnce);
        }
    }
}
