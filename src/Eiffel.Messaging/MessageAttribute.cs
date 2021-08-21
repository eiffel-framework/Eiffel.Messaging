using System;

namespace Eiffel.Messaging
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MessageAttribute : Attribute
    {
        public string Route { get; private set; }

        public MessageAttribute(string route)
        {
            Route = route;
        }
    }
}
