using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Moq;
using Xunit;
using Autofac;
using FluentAssertions;

using Eiffel.Messaging.Abstractions;
using Eiffel.Messaging.DependencyInjection.Autofac;
using Eiffel.Messaging.Exceptions;

namespace Eiffel.Messaging.Tests
{
    public class DependencyInjection_Autofac_Unit_Tests
    {
        private readonly ContainerBuilder _containerBuilder;

        private readonly Mock<IMessageBrokerClient> _mockMessageBrokerClient;
        private readonly Mock<IMessageRouteRegistry> _mockMessageRouteRegistry;
        private readonly Mock<IMessageSerializer> _mockMessageSerializer;
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<IMessageBus> _mockMessageBus;
        private readonly IConfiguration _mockValidConfiguration;
        private readonly IConfiguration _mockInvalidConfiguraton;

        public DependencyInjection_Autofac_Unit_Tests()
        {
            _containerBuilder = new ContainerBuilder();

            _mockMessageRouteRegistry = new Mock<IMessageRouteRegistry>();

            _mockMessageSerializer = new Mock<IMessageSerializer>();

            _mockMessageBrokerClient = new Mock<IMessageBrokerClient>();

            _mockMediator = new Mock<IMediator>();

            _mockMessageBus = new Mock<IMessageBus>();

            var config = new Dictionary<string, string>
            {
                {"Messaging:Mock:Count", "1"},
            };

            _mockValidConfiguration = new ConfigurationBuilder().AddInMemoryCollection(config).Build();

            _mockInvalidConfiguraton = new ConfigurationBuilder().Build();
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
            _containerBuilder.RegisterInstance(_mockMessageRouteRegistry.Object);
            
            _containerBuilder.RegisterInstance(_mockMessageSerializer.Object);

            _containerBuilder.RegisterInstance(_mockValidConfiguration);

            // Act
            _containerBuilder.AddMessageBroker<MockBrokerClient, MockBrokerClientConfig>();

            // Assert
            var container = _containerBuilder.Build();

            container.IsRegistered<IMessageBrokerClient>().Should().Be(true);

            container.Resolve<IMessageBrokerClient>();
        }

        [Fact]
        public void AddMessageBroker_Should_Throw_Exception_When_Configuraion_InValid()
        {
            // Arrange
            _containerBuilder.RegisterInstance(_mockInvalidConfiguraton).As<IConfiguration>();

            // Act
            _containerBuilder.AddMessageBroker<MockBrokerClient, MockBrokerClientConfig>();

            var container = _containerBuilder.Build();

            Func<IMessageBrokerClient> sutResolve = () =>
            {
                ExceptionDispatchInfo dispatchInfo = null;
                try
                {
                    return container.Resolve<IMessageBrokerClient>();
                }
                catch(Exception ex)
                {
                    dispatchInfo = ExceptionDispatchInfo.Capture(ex);
                }

                if (dispatchInfo != null)
                {
                    throw dispatchInfo.SourceException.InnerException;
                }

                return null;
            };

            // Assert
            Assert.Throws<InvalidConfigurationException>(sutResolve);
        }

        [Fact]
        public void AddMessageBus_Should_Register_MessageBus_As_IMessageBus()
        {
            // Arrange
            _containerBuilder.RegisterInstance(_mockMediator.Object);

            _containerBuilder.RegisterInstance(_mockMessageBrokerClient.Object);

            _containerBuilder.RegisterInstance(_mockValidConfiguration);

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
            _containerBuilder.RegisterInstance(_mockMediator.Object);

            _containerBuilder.RegisterInstance(_mockMessageBrokerClient.Object);

            _containerBuilder.RegisterInstance(_mockValidConfiguration);

            // Act
            _containerBuilder.AddEventBus();

            // Assert
            var container = _containerBuilder.Build();

            container.IsRegistered<IEventBus>().Should().Be(true);

            container.Resolve<IEventBus>();
        }

        [Fact]
        public void AddConsumerService_Should_Register_Service()
        {
            // Arrange
            _containerBuilder.RegisterInstance(_mockMessageBus.Object);

            // Act
            _containerBuilder.AddConsumerService<MockMessage>();

            // Assert
            var contaier = _containerBuilder.Build();

            contaier.IsRegistered<IHostedService>().Should().Be(true);

            contaier.Resolve<IHostedService>().Should().NotBeNull();
        }

        [Fact]
        public void AddConsumerServices_Should_Register_Service_ForEach_Message()
        {
            // Arrange
            _containerBuilder.RegisterInstance(_mockMessageBus.Object);

            // Act
            _containerBuilder.AddConsumerServices();

            // Assert
            var contaier = _containerBuilder.Build();

            var services = contaier.Resolve<IEnumerable<IHostedService>>();

            services.Should().NotBeNullOrEmpty();

            services.Count().Should().BeGreaterOrEqualTo(3);
        }
    }
}
