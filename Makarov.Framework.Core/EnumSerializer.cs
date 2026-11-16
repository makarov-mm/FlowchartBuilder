namespace Makarov.Framework.Core;

/// <summary>Serializes any enum by member name; <see cref="Managers.SerializerManager"/> creates one per enum type on demand.</summary>
public sealed class EnumSerializer : AbstractSerializer
{
    private readonly Type _enumType;

    public EnumSerializer(Type enumType)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException($"'{enumType}' is not an enum type.", nameof(enumType));

        _enumType = enumType;
    }

    public override Type SerializerType => _enumType;

    public override string Serialize(object? value) => value?.ToString() ?? string.Empty;

    public override object Deserialize(string text) => Enum.Parse(_enumType, text, ignoreCase: true);
}
