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
        /// Mediator dispatch messages to handlers
        /// <seealso cref="IEventHandler{TPayload}"/>
        /// <seealso cref="IMessageHandler{TPayload}"/>
        /// <seealso cref="ICommandHandler{TPayload}"/>
        /// <seealso cref="ICommandHandler{TPayload, TRetVal}"/>
        /// <seealso cref="IQueryHandler{TPayload, TResult}"/>
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

        /// <summary>
        /// Registers MessageSerializer
        /// Default message serializer Utf8JSON
        /// </summary>
        public static ContainerBuilder AddMessageSerializer(this ContainerBuilder builder)
        {
            builder.RegisterType<DefaultMessageSerializer>()
                .As<IMessageSerializer>()
                .SingleInstance();

            return builder;
        }

        /// <summary>
        /// Registers custom message serializer
        /// </summary>
        /// <typeparam name="TSerializer">Custom IMessageSerializer implementation</typeparam>
        public static ContainerBuilder AddMessageSerializer<TSerializer>(this ContainerBuilder builder)
            where TSerializer : IMessageSerializer
        {
            builder.RegisterType<TSerializer>()
               .As<IMessageSerializer>()
               .SingleInstance();

            return builder;
        }

        /// <summary>
        /// Registers MessageRegistry
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ContainerBuilder AddMessageRegistry(this ContainerBuilder builder)
        {
            builder.RegisterType<MessageRegistry>()
                   .As<IMessageRegistry>()
                   .SingleInstance();

            return builder;
        }

        /// <summary>
        /// Registers messages
        /// Pass generic parameter which messages inherited base class or implemented intraface
        /// <see cref="MessageAttribute"/>
        /// <seealso cref="IMessage"/>
        /// <seealso cref="ICommand"/>
        /// <seealso cref="IEvent"/>
        /// </summary>
        public static ContainerBuilder RegisterMessages<T>(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            var messageTypes = GetMessageTypesFromAssemblies<T>(assemblies);

            var metadataCollection = messageTypes.ToDictionary(x => x, x => x.GetCustomAttribute<MessageAttribute>().GetMetadata());

            builder.RegisterBuildCallback(lifetimeScope =>
            {
                var registry = lifetimeScope.Resolve<IMessageRegistry>();

                messageTypes.ForEach(messageType =>
                {
                    registry.Register(messageType, metadataCollection[messageType]);
                });

            });
            return builder;
        }

        /// <summary>
        /// Registers message
        /// <see cref="MessageAttribute"/>
        /// </summary>
        public static ContainerBuilder RegisterMessage<T>(this ContainerBuilder builder)
        {
            builder.RegisterBuildCallback(lifetimeScope =>
            {
                var registry = lifetimeScope.Resolve<IMessageRegistry>();

                var messageType = typeof(T);

                registry.Register(typeof(T), messageType.GetCustomAttribute<MessageAttribute>().GetMetadata());
            });

            return builder;
        }

        /// <summary>
        /// Registers message brokeer client with configuration
        /// Configuration must be specified appsettings.json under the Messaging section
        /// Messaging must contains broker configuration with name Messaging:Kafka, Messaging:RabbitMQ etc.
        /// After binding validates configuration for misconceptions
        /// </summary>
        public static ContainerBuilder AddMessageBroker<TClient, TConfig>(this ContainerBuilder builder)
            where TClient : IMessageBrokerClient
            where TConfig : IMessageBrokerClientConfig
        {
            var clientConfig = Activator.CreateInstance<TConfig>();

            builder.Register(context =>
            {
                var configuration = context.Resolve<IConfiguration>();

                configuration.Bind($"Messaging:{clientConfig.Name}", clientConfig);

                clientConfig?.Validate();

                var logger = new LoggerFactory().CreateLogger<TClient>();

                var registry = context.Resolve<IMessageRegistry>();

                var serializer = context.Resolve<IMessageSerializer>();

                return Activator.CreateInstance(typeof(TClient), new object[] { logger, clientConfig, registry, serializer });
            }).As<IMessageBrokerClient>()
              .SingleInstance();

            return builder;
        }

        /// <summary>
        /// Resgisters MessageBus
        /// Messages bus uses specified message broker
        /// </summary>
        public static ContainerBuilder AddMessageBus(this ContainerBuilder builder)
        {
            builder.RegisterType<MessageBus>()
                   .As<IMessageBus>()
                   .SingleInstance();

            return builder;
        }

        /// <summary>
        /// Resgisters EventBus
        /// Event bus uses specified message broker
        /// </summary>
        public static ContainerBuilder AddEventBus(this ContainerBuilder builder)
        {
            builder.RegisterType<EventBus>()
                   .As<IEventBus>()
                   .SingleInstance();
            
            return builder;
        }

        /// <summary>
        /// Registers ConsumerService
        /// Consumer services uses Event and Message bus
        /// </summary>
        public static ContainerBuilder AddConsumerService<TMessage>(this ContainerBuilder builder)
            where TMessage : class
        {
            builder.RegisterType<ConsumerService<TMessage>>()
                   .InstancePerDependency()
                   .As<IHostedService>();

            return builder;
        }

        /// <summary>
        /// Registers ConsumerService
        /// Consumer service for each message which messages inherited base class or implemented intraface
        /// </summary>
        public static ContainerBuilder AddConsumerServices<T>(this ContainerBuilder builder, Assembly[] assemblies)
        {
            var messageTypes = GetMessageTypesFromAssemblies<T>(assemblies);

            messageTypes.ForEach(messageType =>
            {
                var consumerService = typeof(ConsumerService<>).MakeGenericType(messageType);

                builder.RegisterType(consumerService)
                       .InstancePerDependency()
                       .As<IHostedService>();
            });

            return builder;
        }

        private static List<Type> GetMessageTypesFromAssemblies<T>(Assembly[] assemblies)
        {
            List<Type> messageTypes = new List<Type>();

            if (typeof(T).IsClass && typeof(T).IsAbstract)
                messageTypes.AddRange(assemblies.SelectMany(x => x.GetTypes().Where(x => x.BaseType == typeof(T))));
            else
                messageTypes.AddRange(assemblies.SelectMany(x => x.GetTypes().Where(x => x.IsAssignableTo(typeof(T)) && x.IsClass)));

            return messageTypes;
        }

        private static ContainerBuilder RegisterHandlers(this ContainerBuilder builder, Assembly[] assemblies = null)
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

        private static ContainerBuilder RegisterPipelines(this ContainerBuilder builder, Assembly[] assemblies = null)
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
    }
}
