using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using Eiffel.Messaging.Abstractions;
using Eiffel.Messaging.DependencyInjection.Autofac;
using Eiffel.Messaging.InMemory;

namespace Eiffel.Messaging.Samples.InMemory
{
    public class Program
    {
        protected Program() { }

        public static async Task Main(string[] args)
        {
            var serviceHost = CreateHostBuilder(args).Build();

            AppDomain.CurrentDomain.UnhandledException += (sender, exception) =>
            {
                var logger = serviceHost.Services.GetRequiredService<ILogger<Program>>();
                logger.LogError((Exception)exception.ExceptionObject, "UnhandledException");
            };

            var registry = serviceHost.Services.GetAutofacRoot().Resolve<IMessageRouteRegistry>();

            registry.Register<InMemoryMessage>("sample-route");

            await serviceHost.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args)
               .ConfigureAppConfiguration(cfg =>
               {
                   var config = new ConfigurationBuilder()
                                        .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                                        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
                                        .AddEnvironmentVariables()
                                        .Build();

                   cfg.AddConfiguration(config);
               })
               .UseWindowsService()
               .UseServiceProviderFactory(new AutofacServiceProviderFactory())
               .ConfigureContainer<ContainerBuilder>(ConfigureContainer);

            return hostBuilder;
        }

        private static void ConfigureContainer(HostBuilderContext builderContext, ContainerBuilder builder)
        {
            builder.AddMediator();
            builder.AddMessageRouteRegistry();
            builder.AddMessageSerializer();
            builder.AddMessageBroker<InMemoryClient, InMemoryClientConfig>();
            builder.AddMessageBus();

            builder.RegisterType<ConsumerService>()
                .As<IHostedService>()
                .InstancePerDependency();

            builder.RegisterType<Worker>()
                .As<IHostedService>()
                .InstancePerDependency();
        }
    }

    public class MessageHandler : IMessageHandler<InMemoryMessage>
    {
        public async Task HandleAsync(InMemoryMessage payload, CancellationToken cancellationToken = default)
        {
            await Console.Out.WriteLineAsync(payload.Message);
        }
    }

    public class InMemoryMessage : IMessage
    {
        public InMemoryMessage(string message)
        {
            Message = message;
        }
        public string Message { get; }
    }
}
