using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blazor.Shared.Converters;

internal sealed class PolymorphicConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsInterface || typeToConvert.IsAbstract;
    }

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter)Activator.CreateInstance(
            typeof(PolymorphicConverter<>).MakeGenericType(typeToConvert),
            BindingFlags.Instance | BindingFlags.Public,
            null,
            Array.Empty<object>(),
            null)!;
    }
}
