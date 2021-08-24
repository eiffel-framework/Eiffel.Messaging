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
using System.Reflection;

namespace Eiffel.Messaging.Tests
{
    public class DependencyInjection_Autofac_Unit_Tests
    {
        private readonly ContainerBuilder _containerBuilder;

        private readonly Mock<IMessageBrokerClient> _mockMessageBrokerClient;
        private readonly Mock<IMessageRegistry> _mockMessageRegistry;
        private readonly Mock<IMessageSerializer> _mockMessageSerializer;
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<IMessageBus> _mockMessageBus;
        private readonly Mock<IEventBus> _mockEventBus;
        private readonly IConfiguration _mockValidConfiguration;
        private readonly IConfiguration _mockInvalidConfiguraton;

        public DependencyInjection_Autofac_Unit_Tests()
        {
            _containerBuilder = new ContainerBuilder();

            _mockMessageRegistry = new Mock<IMessageRegistry>();

            _mockMessageSerializer = new Mock<IMessageSerializer>();

            _mockMessageBrokerClient = new Mock<IMessageBrokerClient>();

            _mockMediator = new Mock<IMediator>();

            _mockMessageBus = new Mock<IMessageBus>();

            _mockEventBus = new Mock<IEventBus>();

            var config = new Dictionary<string, string>
            {
                {"Messaging:Mock:Count", "1"},
            };

            _mockValidConfiguration = new ConfigurationBuilder().AddInMemoryCollection(config).Build();

            _mockInvalidConfiguraton = new ConfigurationBuilder().Build();
        }

        [Fact]
        public void AddMessageRegistry_Should_Register_MessageRouteRegistry_As_IMessageRouteRegistry()
        {
            // Act
            _containerBuilder.AddMessageRegistry();

            // Assert
            var container = _containerBuilder.Build();

            container.IsRegistered<IMessageRegistry>().Should().Be(true);

            container.Resolve<IMessageRegistry>();
        }

        [Fact]
        public void RegisterMessages_Should_Register_Messages()
        {
            // Arrange
            _containerBuilder.AddMessageRegistry();

            // Act
            _containerBuilder.RegisterMessages<IMessage>(new[] { Assembly.GetExecutingAssembly() });

            // Assert
            var container = _containerBuilder.Build();

            var registry = container.Resolve<IMessageRegistry>();

            registry.GetRoute<MockMessage>().Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void RegisterMessage_Should_Register_Message()
        {
            // Arrange
            _containerBuilder.AddMessageRegistry();

            // Act
            _containerBuilder.RegisterMessage<MockMessage>();

            // Assert
            var container = _containerBuilder.Build();

            var registry = container.Resolve<IMessageRegistry>();

            registry.GetRoute<MockMessage>().Should().NotBeNullOrWhiteSpace();
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
        public void AddMessageSerializer_Should_Register_CustomMessageSerializer_As_IMessageSerializer()
        {
            // Act
            _containerBuilder.AddMessageSerializer<MockMessageSerializer>();

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

            container.IsRegistered<IPipelinePreProcessor>().Should().Be(true);

            container.Resolve<IPipelinePreProcessor>();

            container.IsRegistered<IPipelinePostProcessor>().Should().Be(true);

            container.Resolve<IPipelinePostProcessor>();
        }

        [Fact]
        public void AddMessageBroker_Should_Register_MessageBroker_As_IMessageBrokerClient()
        {
            // Arrange
            _containerBuilder.RegisterInstance(_mockMessageRegistry.Object);

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
                catch (Exception ex)
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

            _containerBuilder.RegisterInstance(_mockEventBus.Object);

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

            _containerBuilder.RegisterInstance(_mockEventBus.Object);

            var assemblies = new[] { Assembly.GetExecutingAssembly() };

            // Act
            _containerBuilder.AddConsumerServices<IMessage>(assemblies);
            _containerBuilder.AddConsumerServices<ICommand>(assemblies);
            _containerBuilder.AddConsumerServices<IEvent>(assemblies);

            // Assert
            var contaier = _containerBuilder.Build();

            var services = contaier.Resolve<IEnumerable<IHostedService>>();

            services.Should().NotBeNullOrEmpty();

            services.Count().Should().BeGreaterOrEqualTo(3);
        }
    }
}
