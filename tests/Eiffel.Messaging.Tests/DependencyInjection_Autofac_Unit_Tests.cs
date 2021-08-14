using Autofac;
using Eiffel.Messaging.Abstractions;
using Eiffel.Messaging.DependencyInjection.Autofac;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Eiffel.Messaging.Tests
{
    public class DependencyInjection_Autofac_Unit_Tests
    {
        private readonly ContainerBuilder _containerBuilder;

        public DependencyInjection_Autofac_Unit_Tests()
        {
            _containerBuilder = new ContainerBuilder();

            IConfiguration configuration = new ConfigurationBuilder().Build();

            _containerBuilder.RegisterInstance(configuration).As<IConfiguration>().SingleInstance();
        }

        [Fact]
        public void AddMessageRouteRegistry_Should_Register_MessageRouteRegistry_As_IMessageRouteRegistry()
        {
            // Act
            _containerBuilder.AddMessageRouteRegistry();

            // Assert
            var container = _containerBuilder.Build();

            container.IsRegistered<IMessageRouteRegistry>().Should().Be(true);
            container.Resolve<IMessageRouteRegistry>();
        }

        [Fact]
        public void AddMessageSerializer_Should_Register_MessageSerializer_As_IMessageSerializer()
        {
            // Act
            _containerBuilder.AddMessageSerializer();

            // Assert
            var container = _containerBuilder.Build();

            container.IsRegistered<IMessageSerializer>().Should().Be(true);
            container.Resolve<IMessageSerializer>();
        }

        [Fact]
        public void AddMediator_Should_Register_Mediator_As_IMediator()
        {
            // Act
            _containerBuilder.AddMediator();

            // Assert
            var container = _containerBuilder.Build();

            container.IsRegistered<IMediator>().Should().Be(true);
            container.Resolve<IMediator>();
        }

        [Fact]
        public void AddMediator_Should_Register_Handlers()
        {
            // Act
            _containerBuilder.AddMediator();

            // Assert
            var container = _containerBuilder.Build();

            container.IsRegistered<IMediator>().Should().Be(true);
            container.Resolve<IMediator>();

            container.IsRegistered<MockCommandHandler>().Should().Be(true);
            container.Resolve<MockCommandHandler>();

            container.IsRegistered<MockEventHandler>().Should().Be(true);
            container.Resolve<MockEventHandler>();

            container.IsRegistered<MockQueryHandler>().Should().Be(true);
            container.Resolve<MockQueryHandler>();

            container.IsRegistered<MockMessageHandler>().Should().Be(true);
            container.Resolve<MockMessageHandler>();
        }

        [Fact]
        public void AddMediator_Should_Register_Pipelines()
        {
            // Act
            _containerBuilder.AddMediator();

            // Assert
            var container = _containerBuilder.Build();

            container.IsRegistered<IMediator>().Should().Be(true);
            container.Resolve<IMediator>();

            container.IsRegistered<MockValidationPipeline>().Should().Be(true);
            container.Resolve<MockValidationPipeline>();

            container.IsRegistered<MockAuditLoggingPipeline>().Should().Be(true);
            container.Resolve<MockAuditLoggingPipeline>();
        }

        [Fact]
        public void AddMessageBroker_Should_Register_MessageBroker_As_IMessageBrokerClient()
        {
            // Arrange
            _containerBuilder.RegisterType<MessageRouteRegistry>().As<IMessageRouteRegistry>().SingleInstance();

            _containerBuilder.RegisterType<DefaultMessageSerializer>().As<IMessageSerializer>();

            // Act
            _containerBuilder.AddMessageBroker<MockBrokerClient, MockBrokerClientConfig>();

            // Assert
            var container = _containerBuilder.Build();

            container.IsRegistered<IMessageBrokerClient>().Should().Be(true);
            container.Resolve<IMessageBrokerClient>();
        }

        [Fact]
        public void AddMessageBus_Should_Register_MessageBus_As_IMessageBus()
        {
            // Arrange

            _containerBuilder.AddMediator();

            _containerBuilder.RegisterType<MessageRouteRegistry>().As<IMessageRouteRegistry>().SingleInstance();

            _containerBuilder.RegisterType<DefaultMessageSerializer>().As<IMessageSerializer>();

            _containerBuilder.AddMessageBroker<MockBrokerClient, MockBrokerClientConfig>();

            // Act
            _containerBuilder.AddMessageBus();

            // Assert
            var container = _containerBuilder.Build();

            container.IsRegistered<IMessageBus>().Should().Be(true);
            container.Resolve<IMessageBus>();
        }

        [Fact]
        public void AddMessageBus_Should_Register_EventBus_As_IEventBus()
        {
            // Arrange
            _containerBuilder.AddMediator();

            _containerBuilder.RegisterType<MessageRouteRegistry>().As<IMessageRouteRegistry>().SingleInstance();

            _containerBuilder.RegisterType<DefaultMessageSerializer>().As<IMessageSerializer>();

            _containerBuilder.AddMessageBroker<MockBrokerClient, MockBrokerClientConfig>();

            // Act
            _containerBuilder.AddEventBus();

            // Assert
            var container = _containerBuilder.Build();

            container.IsRegistered<IEventBus>().Should().Be(true);
            container.Resolve<IEventBus>();
        }
    }
}
