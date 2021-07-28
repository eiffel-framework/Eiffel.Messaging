using Autofac;
using Eiffel.Messaging.Abstractions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Eiffel.Messaging.Tests
{
    public class Mediator_Unit_Tests
    {
        private readonly ContainerBuilder _containerBuilder;

        private IMediator _mediator;

        private readonly MockCommand _mockCommand;
        private readonly Mock<ICommandHandler<MockCommand>> _mockCommandHandler;

        private readonly MockQuery _mockQuery;
        private readonly Mock<IQueryHandler<MockQuery, MockQueryResponse>> _mockQueryHandler;

        private readonly MockMessage _mockMessage;
        private readonly Mock<IMessageHandler<MockMessage>> _mockMessageHandler;

        private readonly MockEvent _mockEvent;
        private readonly Mock<IEventHandler<MockEvent>> _mockEventHandler;

        private readonly Mock<IPipelinePreProcessor> _mockPreProcessor;
        private readonly Mock<IPipelinePostProcessor> _mockPostProcessor;

        public Mediator_Unit_Tests()
        {
            _containerBuilder = new ContainerBuilder();

            _mockCommand = new MockCommand();
            _mockCommandHandler = new Mock<ICommandHandler<MockCommand>>();
            _mockCommandHandler.Setup(x => x.HandleAsync(It.IsAny<MockCommand>(), It.IsAny<CancellationToken>()));

            _mockQuery = new MockQuery();
            _mockQueryHandler = new Mock<IQueryHandler<MockQuery, MockQueryResponse>>();
            _mockQueryHandler.Setup(x => x.HandleAsync(It.IsAny<MockQuery>(), It.IsAny<CancellationToken>()));

            _mockMessage = new MockMessage();
            _mockMessageHandler = new Mock<IMessageHandler<MockMessage>>();
            _mockMessageHandler.Setup(x => x.HandleAsync(It.IsAny<MockMessage>(), It.IsAny<CancellationToken>()));

            _mockEvent = new MockEvent();
            _mockEventHandler = new Mock<IEventHandler<MockEvent>>();
            _mockEventHandler.Setup(x => x.HandleAsync(It.IsAny<MockEvent>(), It.IsAny<CancellationToken>()));

            _mockPreProcessor = new Mock<IPipelinePreProcessor>();
            _mockPreProcessor.Setup(x => x.ProcessAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()));

            _mockPostProcessor = new Mock<IPipelinePostProcessor>();
            _mockPostProcessor.Setup(x => x.ProcessAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()));

            _containerBuilder.Register<IMediator>(context =>
            {
                return new Mediator(context.Resolve<ILifetimeScope>());
            }).SingleInstance();
        }

        [Fact]
        public async Task Mediator_Should_Throws_Exception_When_Operation_Cancelled()
        {
            // Arrange
            var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));

            await Task.Delay(1);

            _containerBuilder.RegisterInstance(_mockCommandHandler.Object);
            _containerBuilder.RegisterInstance(_mockQueryHandler.Object);
            _containerBuilder.RegisterInstance(_mockMessageHandler.Object);
            _containerBuilder.RegisterInstance(_mockEventHandler.Object);

            _mediator = _containerBuilder.Build().Resolve<IMediator>();

            // Act
            Func<Task> act1 = () => _mediator.SendAsync(_mockCommand, tokenSource.Token);
            Func<Task> act2 = () => _mediator.SendAsync(_mockQuery, tokenSource.Token);
            Func<Task> act3 = () => _mediator.DispatchAsync(_mockMessage, tokenSource.Token);
            Func<Task> act4 = () => _mediator.PublishAsync(_mockEvent, tokenSource.Token);

            // Assert
            await Assert.ThrowsAsync<OperationCanceledException>(act1);
            await Assert.ThrowsAsync<OperationCanceledException>(act2);
            await Assert.ThrowsAsync<OperationCanceledException>(act3);
            await Assert.ThrowsAsync<OperationCanceledException>(act4);

            _mockCommandHandler.Verify(x => x.HandleAsync(It.IsAny<MockCommand>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockQueryHandler.Verify(x => x.HandleAsync(It.IsAny<MockQuery>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockMessageHandler.Verify(x => x.HandleAsync(It.IsAny<MockMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockEventHandler.Verify(x => x.HandleAsync(It.IsAny<MockEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Mediator_Should_Throws_Exception_When_Handler_Not_Registered()
        {
            // Arrange
            _mediator = _containerBuilder.Build().Resolve<IMediator>();

            // Act
            Func<Task> act1 = () => _mediator.SendAsync(_mockCommand, default);
            Func<Task> act2 = () => _mediator.SendAsync(_mockQuery, default);
            Func<Task> act3 = () => _mediator.DispatchAsync(_mockMessage, default);
            Func<Task> act4 = () => _mediator.PublishAsync(_mockEvent, default);

            // Assert
            await Assert.ThrowsAsync<HandlerNotFoundException>(act1);
            await Assert.ThrowsAsync<HandlerNotFoundException>(act2);
            await Assert.ThrowsAsync<HandlerNotFoundException>(act3);
            await Assert.ThrowsAsync<HandlerNotFoundException>(act4);

            _mockCommandHandler.Verify(x => x.HandleAsync(It.IsAny<MockCommand>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockQueryHandler.Verify(x => x.HandleAsync(It.IsAny<MockQuery>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockMessageHandler.Verify(x => x.HandleAsync(It.IsAny<MockMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockEventHandler.Verify(x => x.HandleAsync(It.IsAny<MockEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Mediator_Should_Process_Pipelines()
        {
            // Arrange
            _containerBuilder.RegisterInstance(_mockCommandHandler.Object);
            _containerBuilder.RegisterInstance(_mockQueryHandler.Object);
            _containerBuilder.RegisterInstance(_mockMessageHandler.Object);

            _containerBuilder.RegisterInstance(_mockPreProcessor.Object);
            _containerBuilder.RegisterInstance(_mockPostProcessor.Object);

            _mediator = _containerBuilder.Build().Resolve<IMediator>();

            // Act
            await _mediator.SendAsync(_mockCommand, default);
            await _mediator.SendAsync(_mockQuery, default);
            await _mediator.DispatchAsync(_mockMessage, default);

            // Assert
            _mockCommandHandler.Verify(x => x.HandleAsync(It.IsAny<MockCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryHandler.Verify(x => x.HandleAsync(It.IsAny<MockQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockMessageHandler.Verify(x => x.HandleAsync(It.IsAny<MockMessage>(), It.IsAny<CancellationToken>()), Times.Once);

            _mockPreProcessor.Verify(x => x.ProcessAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
            _mockPostProcessor.Verify(x => x.ProcessAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        }

        [Fact]
        public async Task Mediator_Should_Dispatch_Success()
        {
            // Arrange
            _containerBuilder.RegisterInstance(_mockCommandHandler.Object);
            _containerBuilder.RegisterInstance(_mockQueryHandler.Object);
            _containerBuilder.RegisterInstance(_mockMessageHandler.Object);
            _containerBuilder.RegisterInstance(_mockEventHandler.Object);

            _mediator = _containerBuilder.Build().Resolve<IMediator>();

            // Act
            await _mediator.SendAsync(_mockCommand, default);
            await _mediator.SendAsync(_mockQuery, default);
            await _mediator.DispatchAsync(_mockMessage, default);
            await _mediator.PublishAsync(_mockEvent, default);

            // Assert
            _mockCommandHandler.Verify(x => x.HandleAsync(It.IsAny<MockCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryHandler.Verify(x => x.HandleAsync(It.IsAny<MockQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockMessageHandler.Verify(x => x.HandleAsync(It.IsAny<MockMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventHandler.Verify(x => x.HandleAsync(It.IsAny<MockEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
