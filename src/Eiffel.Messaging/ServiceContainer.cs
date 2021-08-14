using Autofac;
using Eiffel.Messaging.Abstractions;
using System;

namespace Eiffel.Messaging
{
    /// <summary>
    /// Service container supported multiple dependency injection
    /// </summary>
    public class ServiceContainer : IServiceContainer
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly IComponentContext _componentContext;

        public ServiceContainer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ServiceContainer(IComponentContext componentContext)
        {
            _componentContext = componentContext;
        }

        /// <summary>
        /// Check service is registered or not
        /// </summary>
        public bool IsRegistered<TService>()
        {
            return _serviceProvider?.GetService(typeof(TService)) != null || (_componentContext?.IsRegistered<TService>() ?? false);
        }

        /// <summary>
        /// Check service is registered or not
        /// </summary>
        public bool IsRegistered(Type type)
        {
            return _serviceProvider?.GetService(type) != null || (_componentContext?.IsRegistered(type) ?? false);
        }

        /// <summary>
        /// Resolve service
        /// </summary>
        public TService Resolve<TService>()
        {
            return (TService)_serviceProvider?.GetService(typeof(TService)) ?? _componentContext.Resolve<TService>();
        }

        /// <summary>
        /// Resolve service
        /// </summary>
        public object Resolve(Type type)
        {
            return _serviceProvider?.GetService(type) ?? _componentContext.Resolve(type);
        }
    }
}