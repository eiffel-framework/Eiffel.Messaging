using System;
using System.Collections.Generic;

namespace Eiffel.Messaging.Abstractions
{
    /// <summary>
    /// Message route registry
    /// </summary>
    public interface IMessageRegistry
    {
        /// <summary>
        /// Registers message metadata
        /// </summary>
        void Register<TMessage>(IMessageMetadata metadata) where TMessage : class;

        /// <summary>
        /// Registers message metadata
        /// </summary>
        void Register(Type messageType, IMessageMetadata metadata);

        /// <summary>
        /// Registers message
        /// </summary>
        void Register(Type type);

        /// Registers message
        /// </summary>
        void Register<TMessage>() where TMessage : class;

        /// <summary>
        /// Returns specified message route
        /// </summary>
        string GetRoute<TMessage>() where TMessage : class;

        /// <summary>
        /// Returns specified message route
        /// </summary>
        string GetRoute(Type messageType);

        /// <summary>
        /// Returns specified message routes
        /// </summary>
        List<string> GetRoutes<TMessage>() where TMessage : class;

        /// <summary>
        /// Returns specified message routes
        /// </summary>
        List<string> GetRoutes(Type messageType);

        /// <summary>
        /// Returns message metadata
        /// </summary>
        IMessageMetadata Get<TMessage>() where TMessage : class;

        /// <summary>
        /// Returns message metadata
        /// </summary>
        IMessageMetadata Get(Type messageType);
    }
}
