using System.ComponentModel;
using System.Globalization;

namespace Makarov.FlowchartBuilder.Glyphs;

internal sealed class RoutingConverter() : EnumConverter(typeof(ConnectorRouting))
{
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
        destinationType == typeof(string) && value is ConnectorRouting route
            ? Core.Instance.CurrentTranslator[route.ToString()]
            : base.ConvertTo(context, culture, value, destinationType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string text)
            foreach (var route in Enum.GetValues<ConnectorRouting>())
                if (Core.Instance.CurrentTranslator[route.ToString()] == text) return route;
        return base.ConvertFrom(context, culture, value);
    }
}
