using System;
using System.Reflection;

using Xunit;
using FluentAssertions;

using Eiffel.Messaging.Abstractions;

namespace Eiffel.Messaging.Tests
{
    public class MessageRegistry_Unit_Tests
    {
        private readonly IMessageRegistry _messageRouteRegistry;

        public MessageRegistry_Unit_Tests()
        {
            _messageRouteRegistry = new MessageRegistry();
        }
        
    }
}
