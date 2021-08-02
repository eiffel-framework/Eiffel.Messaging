using System;
using System.Collections.Generic;
using System.Linq;

namespace Eiffel.Messaging.Abstractions
{
    /// <summary>
    /// Register messages and routes
    /// </summary>
    public interface IMessageRouteRegistry
    {
        /// <summary>
        /// Returns route of messages
        /// </summary>
        /// <exception cref="KeyNotFoundException"></exception>
        string GetRoute<TMessage>()
            where TMessage : class;
        
        /// <summary>
        /// Registers message for route
        /// </summary>
        void Register<TMessage>(string route);

        /// <summary>
        /// Registers message(s) for route
        /// </summary>
        void Register(string route, Type[] types);

        HashSet<Tuple<Type, string>> Routes { get; }
    }
}
