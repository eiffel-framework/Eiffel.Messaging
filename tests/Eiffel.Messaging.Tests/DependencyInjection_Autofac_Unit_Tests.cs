
using Autofac;
using Eiffel.Messaging.Abstractions;
using Eiffel.Messaging.DependencyInjection.Autofac;
using FluentAssertions;
using Xunit;

namespace Eiffel.Messaging.Tests
{
    public class DependencyInjection_Autofac_Unit_Tests
    {
        private readonly ContainerBuilder _containerBuilder;

        public DependencyInjection_Autofac_Unit_Tests()
        {
            // Arrange
            _containerBuilder = new ContainerBuilder();
        }

        [Fact]
        public void AddMediator_Should_Register_Mediator_As_IMediator()
        {
            // Act
            _containerBuilder.AddMediator();

            // Assert
            var container = _containerBuilder.Build();

            container.IsRegistered<IMediator>().Should().Be(true);
        }

        [Fact]
        public void AddMediator_Should_Register_Handlers()
        {
            // Act
            _containerBuilder.AddMediator();

            // Assert
            var container = _containerBuilder.Build();

            container.IsRegistered<IMediator>().Should().Be(true);

            container.IsRegistered<MockCommandHandler>().Should().Be(true);

            container.IsRegistered<MockEventHandler>().Should().Be(true);

            container.IsRegistered<MockQueryHandler>().Should().Be(true);

            container.IsRegistered<MockMessageHandler>().Should().Be(true);

        }

        [Fact]
        public void AddMediator_Should_Register_Pipelines()
        {
            // Act
            _containerBuilder.AddMediator();

            // Assert
            var container = _containerBuilder.Build();

            container.IsRegistered<IMediator>().Should().Be(true);

            container.IsRegistered<MockValidationPipeline>().Should().Be(true);

            container.IsRegistered<MockAuditLoggingPipeline>().Should().Be(true);
        }

        [Fact]
        public void AddMessageBroker_Should_Register_MessageBroker_As_IMessageBrokerClient()
        {
            // Act
            _containerBuilder.AddMessageBroker<MockBrokerClient, MockBrokerClientConfig>();

            // Assert
            var container = _containerBuilder.Build();

            container.IsRegistered<IMessageBrokerClient>().Should().Be(true);
        }

        [Fact]
        public void AddMessageBus_Should_Register_MessageBus_As_IMessageBus()
        {
            // Arrange

            _containerBuilder.AddMediator();

            _containerBuilder.AddMessageBroker<MockBrokerClient, MockBrokerClientConfig>();

            // Act
            _containerBuilder.AddMessageBus();

            // Assert
            var container = _containerBuilder.Build();

            container.IsRegistered<IMessageBus>().Should().Be(true);
        }

        [Fact]
        public void AddMessageBus_Should_Register_EventBus_As_IEventBus()
        {
            // Arrange

            _containerBuilder.AddMediator();

            _containerBuilder.AddMessageBroker<MockBrokerClient, MockBrokerClientConfig>();

            // Act
            _containerBuilder.AddEventBus();

            // Assert
            var container = _containerBuilder.Build();

            container.IsRegistered<IEventBus>().Should().Be(true);
        }
    }
}
