using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Autofac;

using Eiffel.Messaging.Abstractions;
using System.Collections.Generic;

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

            builder.Register(context =>
            {
                return new ServiceContainer(context.Resolve<ILifetimeScope>());
            }).As<IServiceContainer>().SingleInstance();

            builder.RegisterType<Mediator>().As<IMediator>().SingleInstance();

            builder.RegisterHandlers(assemblies);

            builder.RegisterPipelines(assemblies);

            return builder;
        }

        public static ContainerBuilder RegisterHandlers(this ContainerBuilder builder, Assembly[] assemblies = null)
        {
            builder.RegisterAssemblyTypes(assemblies)
               .AsClosedTypesOf(typeof(ICommandHandler<>))
               .AsImplementedInterfaces()
               .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(assemblies)
               .AsClosedTypesOf(typeof(IQueryHandler<,>))
               .AsImplementedInterfaces()
               .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(assemblies)
               .AsClosedTypesOf(typeof(IEventHandler<>))
               .AsImplementedInterfaces()
               .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(assemblies)
               .AsClosedTypesOf(typeof(IMessageHandler<>))
               .AsImplementedInterfaces()
               .InstancePerLifetimeScope();

            return builder;
        }

        public static ContainerBuilder RegisterPipelines(this ContainerBuilder builder, Assembly[] assemblies = null)
        {
            builder.RegisterAssemblyTypes(assemblies)
               .AssignableTo(typeof(IPipelinePreProcessor))
               .AsImplementedInterfaces()
               .SingleInstance();

            builder.RegisterAssemblyTypes(assemblies)
               .AssignableTo(typeof(IPipelinePostProcessor))
               .AsImplementedInterfaces()
               .SingleInstance();

            return builder;
        }

        public static ContainerBuilder AddMessageRoutes(this ContainerBuilder builder, Assembly[] assemblies = null)
        {
            var messageRoutes = GetMessageRoutesFromAssemblies(assemblies);

            builder.Register(x =>
            {
                return new MessageRouteRegistry(messageRoutes);
            }).As<IMessageRouteRegistry>().SingleInstance();

            return builder;
        }

        public static ContainerBuilder AddMessageSerializer(this ContainerBuilder builder)
        {
            builder.RegisterType<DefaultMessageSerializer>()
                .As<IMessageSerializer>()
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

                configuration.Bind($"Messaging:{config.Name}", config);

                config?.Validate();

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

        public static ContainerBuilder AddConsumerService<TMessage>(this ContainerBuilder builder)
            where TMessage : class
        {
            builder.RegisterType<ConsumerService<TMessage>>()
                .InstancePerDependency()
                .As<IHostedService>();

            return builder;
        }

        public static ContainerBuilder AddConsumerServices(this ContainerBuilder builder, Assembly[] assemblies = null)
        {
            if (assemblies == null)
            {
                assemblies = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                    .Select(Assembly.LoadFrom)
                    .ToArray();
            }

            var messageTypes = GetMessageTypesFromAssemblies(assemblies);

            foreach(var messageType in messageTypes)
            {
                var consumerService = typeof(ConsumerService<>).MakeGenericType(messageType);
                builder.RegisterType(consumerService).InstancePerDependency().As<IHostedService>();
            }

            return builder;
        }

        private static Dictionary<Type, string> GetMessageRoutesFromAssemblies(Assembly[] assemblies = null)
        {
            var messageRoutes = new Dictionary<Type, string>();

            if (assemblies == null)
            {
                assemblies = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                    .Select(Assembly.LoadFrom)
                    .ToArray();
            }

            var messageTypes = GetMessageTypesFromAssemblies(assemblies);

            messageTypes.ForEach(x =>
            {
                var messageRouteAttribute = x.GetCustomAttribute<MessageRouteAttribute>();

                if (messageRoutes.ContainsKey(x) || messageRouteAttribute == null) return;

                messageRoutes.Add(x, messageRouteAttribute.Route);
            });

            return messageRoutes;
        }

        private static List<Type> GetMessageTypesFromAssemblies(Assembly[] assemblies)
        {
            return assemblies.SelectMany(x => x.GetTypes().Where(x =>
                           (x.IsAssignableTo(typeof(IMessage)) ||
                            x.IsAssignableTo(typeof(ICommand)) ||
                            x.IsAssignableTo(typeof(IEvent))) && x.IsClass)).ToList() ?? new List<Type>();
        }
    }
}
