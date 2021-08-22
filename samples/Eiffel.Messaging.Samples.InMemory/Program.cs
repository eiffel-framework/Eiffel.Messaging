using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using Eiffel.Messaging.DependencyInjection.Autofac;
using Eiffel.Messaging.InMemory;
using Eiffel.Messaging.Abstractions;
using System.Reflection;

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
            var assembly = Assembly.GetExecutingAssembly();

            builder.AddMediator();
            builder.AddMessageRegistry();
            builder.AddMessageSerializer();
            builder.AddMessageBroker<InMemoryClient, InMemoryClientConfig>();
            builder.AddMessageBus();
            builder.RegisterMessages<IMessage>(new[] { assembly });
            builder.AddConsumerServices<IMessage>(new[] { assembly });

            builder.RegisterType<Worker>()
                .As<IHostedService>()
                .InstancePerDependency();
        }
    }
}
