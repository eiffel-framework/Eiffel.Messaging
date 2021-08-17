using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

        public static IServiceCollection AddMessageRoutes(this IServiceCollection services, Assembly[] assemblies = null)
        {
            var messageRoutes = GetMessageRoutesFromAssemblies(assemblies);

            services.AddSingleton<IMessageRouteRegistry>(serviceProvider =>
            {
                return new MessageRouteRegistry(messageRoutes);
            });

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

                var registry = serviceProvider.GetRequiredService<IMessageRouteRegistry>();

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

        public static IServiceCollection AddConsumerServices(this IServiceCollection services, Assembly[] assemblies = null)
        {
            if (assemblies == null)
            {
                assemblies = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                    .Select(Assembly.LoadFrom)
                    .ToArray();
            }

            var messageTypes = GetMessageTypesFromAssemblies(assemblies);

            foreach (var messageType in messageTypes)
            {
                var serviceType = typeof(ConsumerService<>).MakeGenericType(messageType);

                services.AddTransient(typeof(IHostedService), serviceType);
            }

            return services;
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
