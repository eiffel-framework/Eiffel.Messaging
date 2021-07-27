using Autofac;
using Eiffel.Messaging.Abstractions;

namespace Eiffel.Messaging.Tests
{
    public class Mediator_Unit_Tests
    {
        private readonly ContainerBuilder _containerBuilder;

        private IMediator mediator =>
            _containerBuilder.Build().Resolve<IMediator>();

        public Mediator_Unit_Tests()
        {
            _containerBuilder = new ContainerBuilder();
            _containerBuilder.Register<IMediator>(context =>
            {
                return new Mediator(context.Resolve<ILifetimeScope>());
            }).SingleInstance();
        }


    }
}
