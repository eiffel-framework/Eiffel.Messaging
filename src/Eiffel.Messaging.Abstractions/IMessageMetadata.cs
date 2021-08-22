using System.Collections.Generic;

namespace Eiffel.Messaging.Abstractions
{
    /// <summary>
    /// Message metadata
    /// </summary>
    public interface IMessageMetadata
    {
        /// <summary>
        /// Message route
        /// </summary>
        public string Route { get; }

        /// <summary>
        /// Multiple message route definition. Use only for Events.
        /// </summary>
        public List<string> Routes { get; }

        /// <summary>
        /// If true, persist message database. Default null
        /// </summary>
        public bool? PersistOnFailure { get; }

        /// <summary>
        /// If true, notify from integrated channel. Default null
        /// </summary>
        public bool? NotifyOnFailure { get; }

        /// <summary>
        /// If true, circuit breaker pattern will execute when consume or produce failed. Default null
        /// </summary>
        public bool? RetryOnFailure { get; set; }

        /// <summary>
        /// EventBus, MessageBus retries produce/consume this message N times. Default null
        /// </summary>
        public int RetryCount { get; }
    }
}
