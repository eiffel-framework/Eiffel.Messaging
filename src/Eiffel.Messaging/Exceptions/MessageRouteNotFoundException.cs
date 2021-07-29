using System;
using System.Runtime.Serialization;

namespace Eiffel.Messaging
{
    [Serializable]
    public class MessageRouteNotFoundException : Exception
    {
        public MessageRouteNotFoundException()
        {
        }

        public MessageRouteNotFoundException(string message) : base(message)
        {
        }

        public MessageRouteNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MessageRouteNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
