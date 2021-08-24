using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Eiffel.Messaging.Abstractions;
using System.Collections.Generic;

namespace Eiffel.Messaging.DependencyInjection.Microsoft
{
    public static class IServiceCollectionExtensions
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
        public static IServiceCollection AddMediator(this IServiceCollection services, Assembly[] assemblies = null)
        {
            if (assemblies == null)
            {
                assemblies = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                    .Select(Assembly.LoadFrom)
                    .ToArray();
            }

            services.AddSingleton<IMediator, Mediator>();

            services.AddSingleton<IServiceContainer>(serviceProvider =>
            {
                return new ServiceContainer(serviceProvider);
            });

            services.RegisterHandlers(assemblies);

            services.RegisterPipelines(assemblies);

            return services;
        }

        /// <summary>
        /// Registers MessageRegistry
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IServiceCollection AddMessageRegistry(this IServiceCollection services)
        {
            services.AddSingleton<IMessageRegistry, MessageRegistry>();

            return services;
        }

        /// <summary>
        /// Registers messages
        /// Pass generic parameter which messages inherited base class or implemented intraface
        /// <see cref="MessageAttribute"/>
        /// <seealso cref="IMessage"/>
        /// <seealso cref="ICommand"/>
        /// <seealso cref="IEvent"/>
        /// </summary>
        public static IServiceCollection RegisterMessages<T>(this IServiceCollection services, Assembly[] assemblies)
        {
            var messageTypes = GetMessageTypesFromAssemblies<T>(assemblies);

            var metadataCollection = messageTypes.ToDictionary(x => x, x => x.GetCustomAttribute<MessageAttribute>().GetMetadata());

            var serviceProvider = services.BuildServiceProvider();

            var registry = serviceProvider.GetRequiredService<IMessageRegistry>();

            messageTypes.ForEach(messageType =>
            {
                registry.Register(messageType, metadataCollection[messageType]);
            });

            services.Replace(new ServiceDescriptor(typeof(IMessageRegistry), registry));

            return services;
        }

        /// <summary>
        /// Registers message
        /// <see cref="MessageAttribute"/>
        /// </summary>
        public static IServiceCollection RegisterMessage<T>(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();

            var registry = serviceProvider.GetRequiredService<IMessageRegistry>();

            var messageType = typeof(T);

            var messageMetadata = messageType.GetCustomAttribute<MessageAttribute>().GetMetadata();
            
            registry.Register(messageType, messageMetadata);

            services.Replace(new ServiceDescriptor(typeof(IMessageRegistry), registry));

            return services;
        }

        /// <summary>
        /// Registers MessageSerializer
        /// Default message serializer Utf8JSON
        /// </summary>
        public static IServiceCollection AddMessageSerializer(this IServiceCollection services)
        {
            services.AddSingleton<IMessageSerializer, DefaultMessageSerializer>();

            return services;
        }

        /// <summary>
        /// Registers custom message serializer
        /// </summary>
        /// <typeparam name="TSerializer">Custom IMessageSerializer implementation</typeparam>
        public static IServiceCollection AddMessageSerializer<TSerializer>(this IServiceCollection services)
            where TSerializer : class, IMessageSerializer
        {
            services.AddSingleton<IMessageSerializer, TSerializer>();

            return services;
        }

        /// <summary>
        /// Registers message brokeer client with configuration
        /// Configuration must be specified appsettings.json under the Messaging section
        /// Messaging must contains broker configuration with name Messaging:Kafka, Messaging:RabbitMQ etc.
        /// After binding validates configuration for misconceptions
        /// </summary>
        public static IServiceCollection AddMessageBroker<TClient, TConfig>(this IServiceCollection services)
           where TClient : IMessageBrokerClient
           where TConfig : IMessageBrokerClientConfig
        {
            var config = Activator.CreateInstance<TConfig>();

            services.AddSingleton(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                configuration.Bind($"Messaging:{config.Name}", config);

                config?.Validate();

                var logger = new LoggerFactory().CreateLogger<TClient>();

                var registry = serviceProvider.GetRequiredService<IMessageRegistry>();

                var serializer = serviceProvider.GetRequiredService<IMessageSerializer>();

                return (IMessageBrokerClient)Activator.CreateInstance(typeof(TClient), new object[] { logger, config, registry, serializer });
            });

            return services;
        }

        /// <summary>
        /// Resgisters MessageBus
        /// Messages bus uses specified message broker
        /// </summary>
        public static IServiceCollection AddMessageBus(this IServiceCollection services)
        {
            services.AddSingleton<IMessageBus>(serviceProvider =>
            {
                var mediator = serviceProvider.GetRequiredService<IMediator>();

                var client = serviceProvider.GetRequiredService<IMessageBrokerClient>();

                return new MessageBus(mediator, client);
            });

            return services;
        }

        /// <summary>
        /// Resgisters EventBus
        /// Event bus uses specified message broker
        /// </summary>
        public static IServiceCollection AddEventBus(this IServiceCollection services)
        {
            services.AddSingleton<IEventBus>(serviceProvider =>
            {
                var mediator = serviceProvider.GetRequiredService<IMediator>();

                var client = serviceProvider.GetRequiredService<IMessageBrokerClient>();

                return new EventBus(mediator, client);
            });

            return services;
        }

        /// <summary>
        /// Registers ConsumerService
        /// Consumer services uses Event and Message bus
        /// </summary>
        public static IServiceCollection AddConsumerService<TMessage>(this IServiceCollection services)
            where TMessage : class
        {
            services.AddHostedService<ConsumerService<TMessage>>();
            return services;
        }

        /// <summary>
        /// Registers ConsumerService
        /// Consumer service for each message which messages inherited base class or implemented intraface
        /// </summary>
        public static IServiceCollection AddConsumerServices<T>(this IServiceCollection services, Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                assemblies = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                    .Select(Assembly.LoadFrom)
                    .ToArray();
            }

            var messageTypes = GetMessageTypesFromAssemblies<T>(assemblies);

            foreach (var messageType in messageTypes)
            {
                var serviceType = typeof(ConsumerService<>).MakeGenericType(messageType);

                services.AddTransient(typeof(IHostedService), serviceType);
            }

            return services;
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

        private static IServiceCollection RegisterHandlers(this IServiceCollection services, Assembly[] assemblies)
        {
            services.Scan(x => x.FromAssemblies(assemblies)
                .AddClasses(x => x.AssignableTo(typeof(ICommandHandler<>)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime());

            services.Scan(x => x.FromAssemblies(assemblies)
                .AddClasses(x => x.AssignableTo(typeof(ICommandHandler<,>)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime());

            services.Scan(x => x.FromAssemblies(assemblies)
                .AddClasses(x => x.AssignableTo(typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime());

            services.Scan(x => x.FromAssemblies(assemblies)
                .AddClasses(x => x.AssignableTo(typeof(IEventHandler<>)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime());

            services.Scan(x => x.FromAssemblies(assemblies)
                .AddClasses(x => x.AssignableTo(typeof(IMessageHandler<>)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime());

            return services;
        }

        private static IServiceCollection RegisterPipelines(this IServiceCollection services, Assembly[] assemblies = null)
        {
            services.Scan(x => x.FromAssemblies(assemblies)
               .AddClasses(x => x.AssignableTo(typeof(IPipelinePreProcessor)))
               .AsImplementedInterfaces()
               .WithSingletonLifetime());

            services.Scan(x => x.FromAssemblies(assemblies)
               .AddClasses(x => x.AssignableTo(typeof(IPipelinePostProcessor)))
               .AsImplementedInterfaces()
               .WithSingletonLifetime());

            return services;
        }
    }
}
