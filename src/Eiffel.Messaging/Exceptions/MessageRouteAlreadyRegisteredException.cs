using System;
using System.Runtime.Serialization;

namespace Eiffel.Messaging
{
    [Serializable]
    public class MessageRouteAlreadyRegisteredException : Exception
    {
        public MessageRouteAlreadyRegisteredException()
        {
        }

        public MessageRouteAlreadyRegisteredException(string message) : base(message)
        {
        }

        public MessageRouteAlreadyRegisteredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MessageRouteAlreadyRegisteredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
