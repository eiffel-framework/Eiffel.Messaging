using System;

namespace Eiffel.Messaging
{
    [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MessageRouteAttribute : Attribute
    {
        public string Route { get; private set; }

        public MessageRouteAttribute(string route)
        {
            Route = route;
        }
    }
}
