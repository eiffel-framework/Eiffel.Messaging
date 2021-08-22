using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Eiffel.Messaging.Kafka;
using Eiffel.Messaging.DependencyInjection.Microsoft;
using Eiffel.Messaging.Abstractions;
using System.Reflection;

namespace Eiffel.Messaging.Samples.Kafka
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

            //var s = serviceHost.Services.GetRequiredService<IMessageRegistry>();
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
               .ConfigureServices(ConfigureServices)
               .UseWindowsService();

            return hostBuilder;
        }

        public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddMediator();
            services.AddMessageRegistry();
            services.RegisterMessages<ICommand>(new[] { Assembly.GetExecutingAssembly() });
            services.AddMessageSerializer();
            services.AddMessageBroker<KafkaClient, KafkaClientConfig>();
            services.AddMessageBus();
            services.AddConsumerService<CreateOrder>();
            services.AddHostedService<Worker>();
        }
    }
}
