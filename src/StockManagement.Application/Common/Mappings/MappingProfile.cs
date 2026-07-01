using System.Reflection;

namespace StockManagement.Application.Common.Mappings;

public static class MappingProfile
{
    public static void ApplyMappingsFromAssembly(Microsoft.Extensions.DependencyInjection.IServiceCollection services, Assembly assembly)
    {
        var mapFromType = typeof(IMapFrom<>);

        var types = assembly.GetExportedTypes()
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == mapFromType))
            .ToList();

        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);
            var methodInfo = type.GetMethod("Mapping")
                ?? type.GetInterface("IMapFrom`1")!.GetMethod("Mapping");
            methodInfo?.Invoke(instance, new object[] { });
        }
    }
}
