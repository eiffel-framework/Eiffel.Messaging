using System;

namespace Eiffel.Messaging.Abstractions
{
    public interface IServiceContainer
    {
        TService Resolve<TService>();

        object Resolve(Type type);

        bool IsRegistered<TService>();

        bool IsRegistered(Type type);
    }
}
