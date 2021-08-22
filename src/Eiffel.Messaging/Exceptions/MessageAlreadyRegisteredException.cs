using System;
using System.Runtime.Serialization;

namespace Eiffel.Messaging
{
    [Serializable]
    public class MessageAlreadyRegisteredException : Exception
    {
        public MessageAlreadyRegisteredException()
        {
        }

        public MessageAlreadyRegisteredException(string message) : base(message)
        {
        }

        public MessageAlreadyRegisteredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MessageAlreadyRegisteredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
