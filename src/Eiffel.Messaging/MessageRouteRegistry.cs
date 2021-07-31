using Eiffel.Messaging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eiffel.Messaging
{
    public class MessageRouteRegistry : IMessageRouteRegistry
    {
        protected List<Tuple<Type, string>> MessageRoutes;

        public MessageRouteRegistry()
        {
            MessageRoutes = new List<Tuple<Type, string>>();
        }

        public virtual string GetRoute<TMessage>() where TMessage : class
        {
            var messageRoute = MessageRoutes.SingleOrDefault(x => x.Item1 == typeof(TMessage));

            if (messageRoute == null)
            {
                throw new MessageRouteNotFoundException($"{ typeof(TMessage).Name }");
            }

            return messageRoute.Item2;
        }

        public virtual void Register<TMessage>(string route)
        {
            Register(route, typeof(TMessage));
        }

        public virtual void Register(string route, Type[] types)
        {
            foreach(var messageType in types.ToHashSet())
            {
                Register(route, messageType);
            }
        }

        public virtual HashSet<Tuple<Type, string>> Routes
        {
            get {
                return MessageRoutes.ToHashSet();
            }
        }

        private void Register(string route, Type messageType)
        {
            if (string.IsNullOrWhiteSpace(route))
            {
                throw new MessageRouteNullOrWhiteSpaceException();
            }

            if (MessageRoutes.Any(x => x.Item1 == messageType))
            {
                throw new MessageRouteAlreadyRegisteredException($"{messageType.Name} already registered for route");
            }

            MessageRoutes.Add(new Tuple<Type, string>(messageType, route));
        }
    }
}
