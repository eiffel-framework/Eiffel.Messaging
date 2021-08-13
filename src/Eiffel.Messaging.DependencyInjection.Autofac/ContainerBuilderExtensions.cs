using Autofac;
using Eiffel.Messaging.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Eiffel.Messaging.DependencyInjection.Autofac
{
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Register mediator with handlers and pipelines
        /// </summary>
        public static ContainerBuilder AddMediator(this ContainerBuilder builder, Assembly[] assemblies = null)
        {
            if (assemblies == null)
            {
                assemblies = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                    .Select(Assembly.LoadFrom)
                    .ToArray();
            }

            builder.RegisterType<Mediator>().As<IMediator>().SingleInstance();

            builder.RegisterHandlers(assemblies);

            builder.RegisterPipelines(assemblies);

            return builder;
        }

        public static ContainerBuilder RegisterHandlers(this ContainerBuilder builder, Assembly[] assemblies = null)
        {
            builder.RegisterAssemblyTypes(assemblies)
                .AsClosedTypesOf(typeof(ICommandHandler<>))
                .AsSelf()
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(assemblies)
               .AsClosedTypesOf(typeof(IQueryHandler<,>))
               .AsSelf()
               .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(assemblies)
               .AsClosedTypesOf(typeof(IEventHandler<>))
               .AsSelf()
               .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(assemblies)
               .AsClosedTypesOf(typeof(IMessageHandler<>))
               .AsSelf()
               .InstancePerLifetimeScope();

            return builder;
        }

        public static ContainerBuilder RegisterPipelines(this ContainerBuilder builder, Assembly[] assemblies = null)
        {
            builder.RegisterAssemblyTypes(assemblies)
               .AssignableTo(typeof(IPipelinePreProcessor))
               .AsSelf()
               .SingleInstance();

            builder.RegisterAssemblyTypes(assemblies)
              .AssignableTo(typeof(IPipelinePostProcessor))
              .AsSelf()
              .SingleInstance();

            return builder;
        }

        public static ContainerBuilder AddMessageBroker<TClient, TConfig>(this ContainerBuilder builder)
            where TClient : IMessageBrokerClient
            where TConfig : IMessageBrokerClientConfig
        {
            var config = Activator.CreateInstance<TConfig>();

            builder.Register(context =>
            {
                var configuration = context.Resolve<IConfiguration>();

                configuration.Bind($"Eiffel:Messaging:{config.Name}", config);

                var logger = new LoggerFactory().CreateLogger<TClient>();

                var registry = context.Resolve<IMessageRouteRegistry>();

                var serializer = context.Resolve<IMessageSerializer>();

                return Activator.CreateInstance(typeof(TClient), new object[] { logger, config, registry, serializer });
            }).As<IMessageBrokerClient>().SingleInstance();

            return builder;
        }

        public static ContainerBuilder AddMessageBus(this ContainerBuilder builder)
        {
            builder.Register(context =>
            {
                var mediator = context.Resolve<IMediator>();

                var client = context.Resolve<IMessageBrokerClient>();

                return new MessageBus(mediator, client);
            }).As<IMessageBus>().SingleInstance();

            return builder;
        }

        public static ContainerBuilder AddEventBus(this ContainerBuilder builder)
        {
            builder.Register(context =>
            {
                var mediator = context.Resolve<IMediator>();

                var client = context.Resolve<IMessageBrokerClient>();

                return new EventBus(mediator, client);
            }).As<IEventBus>().SingleInstance();

            return builder;
        }
    }
}
