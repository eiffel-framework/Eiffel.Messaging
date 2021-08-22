using System;
using System.Reflection;
using System.Collections.Generic;

using Eiffel.Messaging.Abstractions;

namespace Eiffel.Messaging
{
    public class MessageRegistry : IMessageRegistry
    {
        protected readonly Dictionary<Type, IMessageMetadata> _messageMetadata;

        public MessageRegistry()
        {
            _messageMetadata = new Dictionary<Type, IMessageMetadata>();
        }

        /// <summary>
        /// Registers message metadata
        /// </summary>
        public virtual void Register<TMessage>(IMessageMetadata metadata)
            where TMessage : class
        {
            Register(typeof(TMessage), metadata);
        }

        /// <summary>
        /// Registers message metadata
        /// </summary>
        public virtual void Register(Type messageType, IMessageMetadata metadata)
        {
            if (_messageMetadata.ContainsKey(messageType))
                throw new MessageAlreadyRegisteredException(messageType.Name);

            _messageMetadata.Add(messageType, metadata);
        }

        /// <summary>
        /// Registers message
        /// </summary>
        public void Register<TMessage>()
            where TMessage : class
        {
            var metadata = typeof(TMessage).GetCustomAttribute<MessageAttribute>().GetMetadata();

            if (_messageMetadata.ContainsKey(typeof(TMessage)))
            {
                throw new MessageAlreadyRegisteredException(typeof(TMessage).Name);
            }

            _messageMetadata.Add(typeof(TMessage), metadata);
        }

        /// <summary>
        /// Registers message
        /// </summary>
        public void Register(Type type)
        {
            var metadata = type.GetCustomAttribute<MessageAttribute>().GetMetadata();

            if (_messageMetadata.ContainsKey(type))
            {
                throw new MessageAlreadyRegisteredException(type.Name);
            }

            _messageMetadata.Add(type, metadata);
        }

        /// <summary>
        /// Returns specified message route
        /// </summary>
        public string GetRoute<TMessage>() where TMessage : class
        {
            return GetRoute(typeof(TMessage));
        }

        /// <summary>
        /// Returns specified message route
        /// </summary>
        public string GetRoute(Type messageType)
        {
            if (!_messageMetadata.ContainsKey(messageType))
            {
                throw new KeyNotFoundException($"Route could not be found for specified type {messageType.Name}");
            }

            return _messageMetadata[messageType].Route;
        }

        /// <summary>
        /// Returns specified message routes
        /// </summary>
        public List<string> GetRoutes<TMessage>()
            where TMessage : class
        {
            return GetRoutes(typeof(TMessage));
        }

        /// <summary>
        /// Returns specified message routes
        /// </summary>
        public List<string> GetRoutes(Type messageType)
        {
            if (!_messageMetadata.ContainsKey(messageType))
            {
                throw new KeyNotFoundException($"Route could not be found for specified type {messageType.Name}");
            }

            return _messageMetadata[messageType].Routes;
        }

        /// <summary>
        /// Returns message metadata
        /// </summary>
        public IMessageMetadata Get<TMessage>() where TMessage : class
        {
            return Get(typeof(TMessage));
        }

        /// <summary>
        /// Returns message metadata
        /// </summary>
        public IMessageMetadata Get(Type messageType)
        {
            if (!_messageMetadata.ContainsKey(messageType))
            {

                throw new KeyNotFoundException($"Route could not be found for specified type {messageType.Name}");
            }

            return _messageMetadata[messageType];
        }
    }
}
