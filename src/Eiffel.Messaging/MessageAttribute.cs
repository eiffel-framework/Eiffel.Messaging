using System;
using System.Linq;

using Eiffel.Messaging.Abstractions;

namespace Eiffel.Messaging
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class MessageAttribute : Attribute
    {
        /// <summary>
        /// Message route
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Multiple message route definition. Use only for Events.
        /// </summary>
        public string[] Routes { get; set; }

        /// <summary>
        /// If true, persist message database. Overrides global configuration
        /// </summary>
        public bool PersistOnFailure { get; set; } = true;

        /// <summary>
        /// If true, notify from integrated channel
        /// </summary>
        public bool NotifyOnFailure { get; set; } = true;

        /// <summary>
        /// EventBus, MessageBus retries produce/consume this message N times. Overrides global configuration
        /// </summary>
        public int RetryCount { get; set; } = 5;

        /// <summary>
        /// Returns message metadata
        /// </summary>
        /// <returns></returns>
        public IMessageMetadata GetMetadata()
        {
            return new MessageMetadata
            {
                Route = Route,
                Routes = Routes?.ToList(),
                NotifyOnFailure = NotifyOnFailure,
                PersistOnFailure = PersistOnFailure,
                RetryCount = RetryCount
            };
        }
    }
}
