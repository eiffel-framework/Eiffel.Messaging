using Eiffel.Messaging.Abstractions;
using System;

namespace Eiffel.Messaging.Tests
{
    public class MockMessageSerializer : IMessageSerializer
    {
        public TMessage Deserialize<TMessage>(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public byte[] Serialize<TMessage>(TMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
