using Eiffel.Messaging.Abstractions;
using System;

namespace Eiffel.Messaging.Tests
{
    public class MockBrokerClientConfig : IMessageBrokerClientConfig
    {
        public string Name => "MockClient";

        public void Validate()
        {
            // Nothing
        }
    }
}
