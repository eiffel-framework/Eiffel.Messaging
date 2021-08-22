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


        public static IServiceCollection AddMessageRegistry(this IServiceCollection services)
        {
            services.AddSingleton<IMessageRegistry, MessageRegistry>();

            return services;
        }

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

        public static IServiceCollection AddMessageSerializer(this IServiceCollection services)
        {
            services.AddSingleton<IMessageSerializer, DefaultMessageSerializer>();

            return services;
        }

        public static IServiceCollection RegisterHandlers(this IServiceCollection services, Assembly[] assemblies)
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

        public static IServiceCollection RegisterPipelines(this IServiceCollection services, Assembly[] assemblies = null)
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

        public static IServiceCollection AddConsumerService<TMessage>(this IServiceCollection services)
            where TMessage : class
        {
            services.AddHostedService<ConsumerService<TMessage>>();
            return services;
        }

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
    }
}
