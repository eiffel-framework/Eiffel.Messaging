using Eiffel.Messaging.Abstractions;
using Eiffel.Messaging.DependencyInjection.Microsoft;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Eiffel.Messaging.Tests
{
    public class DependencyInjection_Microsoft_Unit_Tests
    {
        private readonly IServiceCollection _services;

        public DependencyInjection_Microsoft_Unit_Tests()
        {
            // Arrange
            _services = new ServiceCollection();

            _services.AddSingleton<IServiceContainer>(serviceProvider =>
            {
                return new ServiceContainer(serviceProvider);
            });

            IConfiguration configuration = new ConfigurationBuilder().Build();

            _services.AddSingleton(configuration);

            _services.AddSingleton<IMessageRouteRegistry, MessageRouteRegistry>();

            _services.AddSingleton<IMessageSerializer, DefaultMessageSerializer>();
        }

        [Fact]
        public void AddMediator_Should_Register_Mediator_As_IMediator()
        {
            // Act
            _services.AddMediator();

            // Assert
            var serviceProvider = _services.BuildServiceProvider();

            serviceProvider.GetService<IMediator>().Should().NotBeNull();
        }

        [Fact]
        public void AddMediator_Should_Register_Handlers()
        {
            // Act
            _services.AddMediator();

            // Assert
            var serviceProvider = _services.BuildServiceProvider();

            serviceProvider.GetService<IMediator>().Should().NotBeNull();

            serviceProvider.GetService<MockCommandHandler>().Should().NotBeNull();

            serviceProvider.GetService<MockEventHandler>().Should().NotBeNull();

            serviceProvider.GetService<MockQueryHandler>().Should().NotBeNull();

            serviceProvider.GetService<MockMessageHandler>().Should().NotBeNull();

        }

        [Fact]
        public void AddMediator_Should_Register_Pipelines()
        {
            // Act
            _services.AddMediator();

            // Assert
            var serviceProvider = _services.BuildServiceProvider();

            serviceProvider.GetService<IMediator>().Should().NotBeNull();

            serviceProvider.GetService<MockValidationPipeline>().Should().NotBeNull();

            serviceProvider.GetService<MockAuditLoggingPipeline>().Should().NotBeNull();
        }

        [Fact]
        public void AddMessageBroker_Should_Register_MessageBroker_As_IMessageBrokerClient()
        {
            // Act
            _services.AddMessageBroker<MockBrokerClient, MockBrokerClientConfig>();

            // Assert
            var serviceProvider = _services.BuildServiceProvider();

            serviceProvider.GetService<IMessageBrokerClient>().Should().NotBeNull();
        }

        [Fact]
        public void AddMessageBus_Should_Register_MessageBus_As_IMessageBus()
        {
            // Arrange

            _services.AddMediator();

            _services.AddMessageBroker<MockBrokerClient, MockBrokerClientConfig>();

            // Act
            _services.AddMessageBus();

            // Assert
            var serviceProvider = _services.BuildServiceProvider();

            serviceProvider.GetService<IMessageBus>().Should().NotBeNull();
        }

        [Fact]
        public void AddMessageBus_Should_Register_EventBus_As_IEventBus()
        {
            // Arrange

            _services.AddMediator();

            _services.AddMessageBroker<MockBrokerClient, MockBrokerClientConfig>();

            // Act
            _services.AddEventBus();

            // Assert
            var serviceProvider = _services.BuildServiceProvider();

            serviceProvider.GetService<IEventBus>().Should().NotBeNull();
        }

    }
}
