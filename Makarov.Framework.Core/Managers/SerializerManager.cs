using System.Reflection;

namespace Makarov.Framework.Core.Managers;

/// <summary>
/// Registry of <see cref="AbstractSerializer"/> implementations keyed by the type they handle.
/// </summary>
public sealed class SerializerManager
{
    private readonly Dictionary<Type, AbstractSerializer> _serializers = [];

    public SerializerManager(params ReadOnlySpan<Assembly> assemblies)
    {
        foreach (Assembly assembly in assemblies)
            AddFromAssembly(assembly);
    }

    public bool CanSerialize(Type type) => type.IsEnum || _serializers.ContainsKey(type);

    public string Serialize(Type type, object? value) => Get(type).Serialize(value);

    public object Deserialize(Type type, string text) => Get(type).Deserialize(text);

    public void Add(AbstractSerializer serializer)
    {
        if (!_serializers.TryAdd(serializer.SerializerType, serializer))
            throw new InvalidOperationException($"A serializer for '{serializer.SerializerType}' is already registered.");
    }

    /// <summary>Registers every concrete <see cref="AbstractSerializer"/> declared in <paramref name="assembly"/>.</summary>
    public void AddFromAssembly(Assembly assembly)
    {
        foreach (Type type in assembly.GetTypes())
        {
            if (type is { IsAbstract: false, ContainsGenericParameters: false } && type.IsSubclassOf(typeof(AbstractSerializer)))
                Add((AbstractSerializer)Activator.CreateInstance(type)!);
        }
    }

    private AbstractSerializer Get(Type type)
    {
        if (_serializers.TryGetValue(type, out AbstractSerializer? serializer))
            return serializer;

        if (!type.IsEnum)
            throw new KeyNotFoundException($"No serializer is registered for '{type}'.");

        serializer = new EnumSerializer(type);
        _serializers.Add(type, serializer);
        return serializer;
    }
}
