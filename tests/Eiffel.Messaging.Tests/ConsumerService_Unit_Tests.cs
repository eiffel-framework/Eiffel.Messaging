using System.Threading;
using System.Threading.Tasks;

using Moq;
using Xunit;

using Eiffel.Messaging.Abstractions;
using System;
using FluentAssertions;

namespace Eiffel.Messaging.Tests
{
    public class ConsumerService_Unit_Tests
    {
        private readonly Mock<IMessageBus> _mockMessageBus;
        private readonly Mock<IEventBus> _mockEventBus;

        public ConsumerService_Unit_Tests()
        {
            _mockMessageBus = new Mock<IMessageBus>();
            _mockEventBus = new Mock<IEventBus>();
        }

        [Theory]
        [InlineData(typeof(MockEvent))]
        [InlineData(typeof(MockCommand))]
        [InlineData(typeof(MockMessage))]
        public async Task ConsumerService_Should_Consume_When_Service_Started(Type messageType)
        {
            // Arrange
            var isCalled = false;

            var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

            var serviceType = typeof(ConsumerService<>).MakeGenericType(messageType);

            var consumerService = Activator.CreateInstance(serviceType, new object[] { _mockMessageBus.Object, _mockEventBus.Object });

            _mockMessageBus.Setup(x => x.SubscribeAsync<object>(It.IsAny<CancellationToken>())).Callback(() =>
            {
                isCalled = true;
            });
            
            _mockEventBus.Setup(x => x.SubscribeAsync<object>(It.IsAny<CancellationToken>())).Callback(() => 
            {
                isCalled = true;
            });

            var executeMethod = serviceType.GetMethod("ExecuteAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            await ((Task)executeMethod.Invoke(consumerService, new object[] { tokenSource.Token }));

            // Assert
            isCalled.Should().Be(true);
        }
    }
}
