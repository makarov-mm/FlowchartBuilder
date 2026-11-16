using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Puts copies of the selected glyphs (and the connectors between them) into the buffer.</summary>
public sealed class CopyCommand : Command
{
    public override void Run()
    {
        if (Core.Instance.ActiveSheet is not { HasSelection: true } sheet)
            return;

        Core.Instance.Buffer.Clear();
        Core.Instance.Buffer.AddRange(GlyphCloner.Clone(sheet.SelectedGlyphs));
        sheet.UpdateGlyphsCommands();
    }
}
