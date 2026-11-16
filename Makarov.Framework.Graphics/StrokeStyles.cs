using Makarov.Framework.Graphics.Interop;

namespace Makarov.Framework.Graphics;

/// <summary>Direct2D stroke styles shared by all canvases; they are factory resources that live as long as the process.</summary>
internal static class StrokeStyles
{
    private static readonly Dictionary<(DashPattern?, StrokeJoin, StrokeCap), nint> Styles = [];

    /// <summary>The style for <paramref name="stroke"/>, or 0 for the default solid style.</summary>
    public static nint Get(in Stroke stroke)
    {
        if (stroke.IsPlain)
            return 0;

        var key = (stroke.Dash, stroke.Join, stroke.Cap);

        lock (Styles)
        {
            if (Styles.TryGetValue(key, out nint style))
                return style;

            var properties = new Native.StrokeStyleProperties
            {
                StartCap = (int)stroke.Cap,
                EndCap = (int)stroke.Cap,
                DashCap = (int)stroke.Cap,
                LineJoin = (int)stroke.Join,
                MiterLimit = 10f,
                DashStyle = stroke.Dash is null ? Native.D2D1_DASH_STYLE_SOLID : Native.D2D1_DASH_STYLE_CUSTOM,
                DashOffset = 0f,
            };

            ReadOnlySpan<float> dashes = stroke.Dash is { } dash ? [dash.Dash, dash.Gap] : [];
            style = D2DFactory.CreateStrokeStyle(properties, dashes);
            Styles.Add(key, style);
            return style;
        }
    }
}
