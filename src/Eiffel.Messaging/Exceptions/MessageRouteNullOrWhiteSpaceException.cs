using System;
using System.Runtime.Serialization;

namespace Eiffel.Messaging
{
    [Serializable]
    public class MessageRouteNullOrWhiteSpaceException : Exception
    {
        public MessageRouteNullOrWhiteSpaceException()
        {
        }

        public MessageRouteNullOrWhiteSpaceException(string message) : base(message)
        {
        }

        public MessageRouteNullOrWhiteSpaceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MessageRouteNullOrWhiteSpaceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
