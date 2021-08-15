using System.Threading;
using System.Threading.Tasks;

using Moq;
using Xunit;

using Eiffel.Messaging.Abstractions;

namespace Eiffel.Messaging.Tests
{
    public class ConsumerService_Unit_Tests
    {
        private readonly Mock<IMessageBus> _mockMessageBus;

        private readonly ConsumerService<MockMessage> _consumerService;

        public ConsumerService_Unit_Tests()
        {
            _mockMessageBus = new Mock<IMessageBus>();

            _consumerService = new ConsumerService<MockMessage>(_mockMessageBus.Object);

        }

        [Fact]
        public async Task ConsumerService_Should_ConsumeMessages_When_Service_Started()
        {
            _mockMessageBus.Setup(x => x.SubscribeAsync<MockMessage>(It.IsAny<CancellationToken>()));

            // Act
            await _consumerService.StartAsync(default);

            await Task.Delay(3000);

            // Assert
            _mockMessageBus.Verify(x => x.SubscribeAsync<MockMessage>(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
    }
}
