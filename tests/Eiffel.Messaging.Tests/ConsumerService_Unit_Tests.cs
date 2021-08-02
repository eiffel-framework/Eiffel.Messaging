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

            var messageRoutes = new HashSet<Tuple<Type, string>>()
            {
                new Tuple<Type, string>(typeof(MockMessage), "route1"),
                new Tuple<Type, string>(typeof(MockEvent), "route2"),
                new Tuple<Type, string>(typeof(MockCommand), "route3")
            };

            _mockRegistry.SetupGet(x => x.Routes).Returns(messageRoutes);

            _consumerService = new ConsumerService(_mockRegistry.Object, _mockMessageBus.Object);

        }

        [Fact]
        public async Task ConsumerService_Should_ConsumeMessages_When_Service_Started()
        {
            // Arrange
            _mockMessageBus.Setup(x => x.SubscribeAsync<dynamic>(It.IsAny<string>(), It.IsAny<CancellationToken>()));

            // Act
            await _consumerService.StartAsync(default);

            // Assert
            _mockMessageBus.Verify(x => x.SubscribeAsync<dynamic>(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
            _mockRegistry.VerifyGet(x => x.Routes, Times.Once);
        }
    }
}
