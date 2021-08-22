using System.Collections.Generic;

using Eiffel.Messaging.Abstractions;

namespace Eiffel.Messaging
{
    internal class MessageMetadata : IMessageMetadata
    {
        public string Route { get; internal set; }

        public List<string> Routes { get; internal set; }

        public bool? PersistOnFailure { get; internal set; }

        public bool? NotifyOnFailure { get; internal set; }

        public int RetryCount { get; internal set; }

        public bool? RetryOnFailure { get; set; }
    }
}
