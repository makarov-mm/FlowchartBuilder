namespace Makarov.FlowchartBuilder.Glyphs;

/// <summary>
/// Copies glyphs for the clipboard. A connector is copied only together with both glyphs it connects,
/// and the copy is attached to their copies.
/// </summary>
public static class GlyphCloner
{
    /// <returns>The copies, in the order of <paramref name="glyphs"/>.</returns>
    public static List<AbstractGlyph> Clone(IEnumerable<AbstractGlyph> glyphs)
    {
        List<AbstractGlyph> originals = [.. glyphs];
        var copies = originals
            .Where(glyph => glyph is not ConnectorGlyph)
            .ToDictionary(glyph => glyph, glyph => glyph.Clone());

        var result = new List<AbstractGlyph>(originals.Count);
        foreach (AbstractGlyph glyph in originals)
        {
            if (glyph is not ConnectorGlyph connector)
            {
                result.Add(copies[glyph]);
            }
            else if (connector.Source is { } source && connector.Target is { } target &&
                     copies.GetValueOrDefault(source) is BlockGlyph sourceCopy &&
                     copies.GetValueOrDefault(target) is BlockGlyph targetCopy)
            {
                result.Add(new ConnectorGlyph(sourceCopy, connector.SourcePort, targetCopy, connector.TargetPort)
                {
                    Caption = connector.Caption,
                });
            }
        }

        return result;
    }
}
