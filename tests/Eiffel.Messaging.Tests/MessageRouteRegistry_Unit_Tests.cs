using Eiffel.Messaging.Abstractions;
using FluentAssertions;
using System;
using Xunit;

namespace Eiffel.Messaging.Tests
{
    public class MessageRouteRegistry_Unit_Tests
    {
        private readonly IMessageRouteRegistry _messageRouteRegistry;

        public MessageRouteRegistry_Unit_Tests()
        {
            _messageRouteRegistry = new MessageRouteRegistry();
        }


        [Fact]
        public void GetMessageRoute_Should_Throws_Exception_When_MessageRoute_Invalid()
        {
            // Arrange
            _messageRouteRegistry.Register<MockMessage>("test");

            // Act
            Func<string> sut = () =>  _messageRouteRegistry.GetRoute<MockCommand>();

            // Assert
            Assert.Throws<MessageRouteNotFoundException>(sut);
        }

        [Fact]
        public void RegisterMessageRoute_Should_Throws_Exception_When_Message_Already_Registered()
        {
            // Arrange
            _messageRouteRegistry.Register<MockMessage>("test");

            // Act
            Action sut = () => _messageRouteRegistry.Register<MockMessage>("test2");

            // Assert
            Assert.Throws<MessageRouteAlreadyRegisteredException>(sut);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void RegisterMessageRoute_Should_Throws_Exception_When_Route_NullOrWhiteSpace(string route)
        {
            // Act
            Action sut = () => _messageRouteRegistry.Register<MockCommand>(route);

            // Assert
            Assert.Throws<MessageRouteNullOrWhiteSpaceException>(sut);
        }

        [Fact]
        public void GetRoute_Should_Success()
        {
            // Arrange
            var route = "test_route";

            _messageRouteRegistry.Register<MockMessage>(route);

            // Act
            var result = _messageRouteRegistry.GetRoute<MockMessage>();

            // Assert
            result.Should().Be(route);
        }

        [Fact]
        public void Register_SingleRoute_Should_Success()
        {
            // Arrange
            _messageRouteRegistry.Register<MockMessage>("test");

            // Act
            var result = _messageRouteRegistry.GetRoute<MockMessage>();

            // Assert
            result.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Register_Multiple_Types_Should_Success()
        {
            // Arrange
            var messageTypes = new[] { typeof(MockMessage), typeof(MockCommand) };

            // Act
            _messageRouteRegistry.Register("test", messageTypes);

            // Assert
            _messageRouteRegistry.GetRoute<MockMessage>().Should().NotBeNullOrWhiteSpace();
            _messageRouteRegistry.GetRoute<MockCommand>().Should().NotBeNullOrWhiteSpace();
        }
    }
}
