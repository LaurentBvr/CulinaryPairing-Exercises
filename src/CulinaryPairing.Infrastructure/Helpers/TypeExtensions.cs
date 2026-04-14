using Mediator;

namespace CulinaryPairing.Infrastructure.Helpers;

public static class TypeExtensions
{
    public static bool IsCommand(this Type type)
    {
        if (type is null) return false;
        if (typeof(ICommand).IsAssignableFrom(type)) return true;
        return type.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
    }

    public static bool IsQuery(this Type type)
    {
        if (type is null) return false;
        return type.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));
    }
}