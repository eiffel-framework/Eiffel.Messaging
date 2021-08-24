using System.Linq;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;
using Xunit;
using FluentAssertions;

using Eiffel.Messaging.Abstractions;
using Eiffel.Messaging.DependencyInjection.Microsoft;
using System;
using Eiffel.Messaging.Exceptions;
using System.Runtime.ExceptionServices;
using System.Reflection;

namespace Eiffel.Messaging.Tests
{
    public class DependencyInjection_Microsoft_Unit_Tests
    {
        private readonly IServiceCollection _services;
        private readonly Mock<IMessageBrokerClient> _mockMessageBrokerClient;
        private readonly Mock<IMessageRegistry> _mockMessageRouteRegistry;
        private readonly Mock<IMessageSerializer> _mockMessageSerializer;
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<IMessageBus> _mockMessageBus;
        private readonly IConfiguration _mockValidConfiguration;
        private readonly IConfiguration _mockInvalidConfiguraton;

        public DependencyInjection_Microsoft_Unit_Tests()
        {
            _services = new ServiceCollection();

            _mockMessageRouteRegistry = new Mock<IMessageRegistry>();

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
        public void AddMediator_Should_Register_Mediator_As_IMediator()
        {
            // Act
            _services.AddMediator();

            // Assert
            var serviceProvider = _services.BuildServiceProvider();

            serviceProvider.GetService<IMediator>().Should().NotBeNull();
        }

        [Fact]
        public void AddMediator_Should_Register_Pipelines()
        {
            // Act
            _services.AddMediator();

            // Assert
            var serviceProvider = _services.BuildServiceProvider();

            serviceProvider.GetService<IMediator>().Should().NotBeNull();

            serviceProvider.GetService<IPipelinePreProcessor>().Should().NotBeNull();

            serviceProvider.GetService<IPipelinePostProcessor>().Should().NotBeNull();
        }

        [Fact]
        public void AddMediator_Should_Register_Handlers()
        {
            // Act
            _services.AddMediator();

            // Assert
            var serviceProvider = _services.BuildServiceProvider();

            serviceProvider.GetService<IMediator>().Should().NotBeNull();

            serviceProvider.GetService<ICommandHandler<MockCommand>>().Should().NotBeNull();

            serviceProvider.GetService<IEventHandler<MockEvent>>().Should().NotBeNull();

            serviceProvider.GetService<IQueryHandler<MockQuery, MockQueryResponse>>().Should().NotBeNull();

            serviceProvider.GetService<IMessageHandler<MockMessage>>().Should().NotBeNull();

        }

        [Fact]
        public void RegisterMessages_Should_Register_Messages()
        {
            // Arrange
            _services.AddMessageRegistry();

            var assemblies = new[] { Assembly.GetExecutingAssembly() };

            // Act
            _services.RegisterMessages<IMessage>(assemblies);
            _services.RegisterMessages<ICommand>(assemblies);
            _services.RegisterMessages<IEvent>(assemblies);

            // Assert
            var serviceProvider = _services.BuildServiceProvider();

            var registry = serviceProvider.GetService<IMessageRegistry>();

            registry.Get<MockMessage>().Route.Should().NotBeNullOrWhiteSpace();
            registry.Get<MockCommand>().Route.Should().NotBeNullOrWhiteSpace();
            registry.Get<MockEvent>().Route.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void RegisterMessage_Should_Register_Message()
        {
            // Arrange
            _services.AddMessageRegistry();


            // Act
            _services.RegisterMessage<MockMessage>();

            // Assert
            var serviceProvider = _services.BuildServiceProvider();

            var registry = serviceProvider.GetService<IMessageRegistry>();

            registry.Get<MockMessage>().Route.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void AddMessageSerializer_Should_Register_MessageSerializer_As_IMessageSerializer()
        {
            // Act
            _services.AddMessageSerializer();

            // Assert
            var serviceProvider = _services.BuildServiceProvider();

            serviceProvider.GetService<IMessageSerializer>().Should().NotBeNull();
        }

        [Fact]
        public void AddMessageSerializer_Should_Register_CustomMessageSerializer_As_IMessageSerializer()
        {
            // Act
            _services.AddMessageSerializer<MockMessageSerializer>();

            // Assert
            var serviceProvider = _services.BuildServiceProvider();

            serviceProvider.GetService<IMessageSerializer>().Should().NotBeNull();
        }

        [Fact]
        public void AddMessageBroker_Should_Register_MessageBroker_As_IMessageBrokerClient()
        {
            // Arrange
            _services.AddSingleton(_mockMessageRouteRegistry.Object);

            _services.AddSingleton(_mockMessageSerializer.Object);

            _services.AddSingleton(_mockValidConfiguration);

            // Act
            _services.AddMessageBroker<MockBrokerClient, MockBrokerClientConfig>();

            // Assert
            var serviceProvider = _services.BuildServiceProvider();

            serviceProvider.GetService<IMessageBrokerClient>().Should().NotBeNull();
        }

        [Fact]
        public void AddMessageBroker_Should_Throw_Exception_When_Configuraion_InValid()
        {
            // Arrange
            _services.AddSingleton(_mockMessageRouteRegistry.Object);

            _services.AddSingleton(_mockMessageSerializer.Object);

            _services.AddSingleton(_mockInvalidConfiguraton);

            _services.AddMessageBroker<MockBrokerClient, MockBrokerClientConfig>();

            var serviceProvider = _services.BuildServiceProvider();

            // Act
            Func<IMessageBrokerClient> sutResolve = () =>
            {
                ExceptionDispatchInfo dispatchInfo = null;
                try
                {
                    return serviceProvider.GetRequiredService<IMessageBrokerClient>();
                }
                catch (Exception ex)
                {
                    dispatchInfo = ExceptionDispatchInfo.Capture(ex);
                }

                if (dispatchInfo != null)
                {
                    throw dispatchInfo.SourceException;
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
            _services.AddSingleton(_mockMediator.Object);

            _services.AddSingleton(_mockMessageBrokerClient.Object);

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
            _services.AddSingleton(_mockMediator.Object);

            _services.AddSingleton(_mockMessageBrokerClient.Object);

            // Act
            _services.AddEventBus();

            // Assert
            var serviceProvider = _services.BuildServiceProvider();

            serviceProvider.GetService<IEventBus>().Should().NotBeNull();
        }

        [Fact]
        public void AddConsumerService_Should_Register_Service()
        {
            // Arrange
            _services.AddSingleton(new Mock<IMessageBus>().Object);

            _services.AddSingleton(new Mock<IEventBus>().Object);

            // Act
            _services.AddConsumerService<MockMessage>();

            // Assert
            var provider = _services.BuildServiceProvider();

            provider.GetService<IHostedService>().Should().NotBeNull();
        }

        [Fact]
        public void AddConsumerServices_Should_Register_Service_ForEach_Message()
        {
            // Arrange
            _services.AddSingleton(new Mock<IMessageBus>().Object);

            _services.AddSingleton(new Mock<IEventBus>().Object);

            // Act
            _services.AddConsumerServices<IMessage>(new[] { Assembly.GetExecutingAssembly() });
            _services.AddConsumerServices<ICommand>(new[] { Assembly.GetExecutingAssembly() });
            _services.AddConsumerServices<IEvent>(new [] { Assembly.GetExecutingAssembly() });

            // Assert
            var provider = _services.BuildServiceProvider();

            var services = provider.GetServices(typeof(IHostedService));

            services.Should().NotBeNullOrEmpty();

            services.Count().Should().BeGreaterOrEqualTo(3);
        }

    }
}
