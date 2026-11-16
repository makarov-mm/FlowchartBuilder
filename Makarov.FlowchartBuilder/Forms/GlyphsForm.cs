using Makarov.FlowchartBuilder.Commands;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Forms;

/// <summary>
/// Palette of the glyph types found in the plugins, grouped by family. Picking an entry arms the glyph
/// for placement with a click on the sheet; a double click adds it right away.
/// </summary>
public partial class GlyphsForm : DockingForm
{
    public GlyphsForm()
    {
        InitializeComponent();
        Core.Instance.CurrentGlyphTypeChanged += OnCurrentGlyphTypeChanged;
    }

    public void ClearGlyphsSelection() => lstView.SelectedIndices.Clear();

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        Core.Instance.CurrentGlyphTypeChanged -= OnCurrentGlyphTypeChanged;
        base.OnFormClosed(e);
    }

    /// <summary>Once the armed glyph is placed, the palette selection is cleared so the same entry can be picked again.</summary>
    private void OnCurrentGlyphTypeChanged(object? sender, EventArgs e)
    {
        if (Core.Instance.CurrentGlyphType is null)
            ClearGlyphsSelection();
    }

    private void GlyphsForm_Load(object? sender, EventArgs e) =>
        Text = Core.Instance.CurrentTranslator["Wnd_Palette_Title"];

    private void GlyphsForm_Shown(object? sender, EventArgs e)
    {
        var glyphs = Core.Instance.GlyphTypes
            .Where(type => GlyphMetadata.Of(type).IsVisible)
            .Select(type => (AbstractGlyph)Activator.CreateInstance(type)!)
            .ToList();

        var images = new ImageList { ColorDepth = ColorDepth.Depth32Bit };
        if (glyphs.Count > 0)
            images.ImageSize = new Size(glyphs.Max(glyph => glyph.ThumbnailWidth), glyphs.Max(glyph => glyph.ThumbnailHeight));

        lstView.LargeImageList = images;
        lstView.ForeColor = Settings.Colors.ThumbnailText;

        foreach (IGrouping<string, AbstractGlyph> family in glyphs.GroupBy(glyph => glyph.Family).OrderBy(family => family.Key))
        {
            ListViewGroup group = lstView.Groups.Add(family.Key, Localize(family.Key));

            foreach (AbstractGlyph glyph in family.OrderBy(glyph => glyph.Metadata.Order ?? int.MaxValue))
            {
                string imageKey = $"{glyph.Family}::{glyph.Name}";
                images.Images.Add(imageKey, glyph.CreateThumbnail());
                lstView.Items.Add(new ListViewItem(Localize(glyph.Name), imageKey) { Tag = glyph.GetType(), Group = group });
            }
        }
    }

    private static string Localize(string name)
    {
        var translator = Core.Instance.CurrentTranslator;
        return translator.Contains(name) ? translator[name] : name;
    }

    private void lstView_ItemSelectionChanged(object? sender, ListViewItemSelectionChangedEventArgs e)
    {
        if (!e.IsSelected || e.Item?.Tag is not Type glyphType)
            return;

        if (GlyphMetadata.Of(glyphType).CommandName is { } commandName)
        {
            switch (Command.GetInstance(commandName))
            {
                case NoneCommand:
                    ClearGlyphsSelection();
                    break;

                case CommandedGlyphCommand glyphCommand:
                    glyphCommand.GlyphType = glyphType;
                    glyphCommand.Execute();
                    break;
            }

            Core.Instance.CurrentGlyphType = null;
        }
        else
        {
            Core.Instance.CurrentGlyphType = glyphType;
        }

        Core.Instance.Redraw();
    }

    private void lstView_MouseDoubleClick(object? sender, MouseEventArgs e)
    {
        // Only blocks can be placed without a position; links need two blocks to connect.
        if (Core.Instance.CurrentGlyphType is not { } glyphType || !glyphType.IsAssignableTo(typeof(BlockGlyph)) ||
            Core.Instance.ActiveSheet is not { } sheet)
        {
            return;
        }

        var glyph = (BlockGlyph)Activator.CreateInstance(glyphType)!;
        if (!glyph.PreparePlacement(this)) return;
        Size viewport = Core.Instance.MainWindow.InnerRectangle.Size;
        glyph.ScaledX = sheet.ScaledViewOrigin.X + viewport.Width / 2;
        glyph.ScaledY = sheet.ScaledViewOrigin.Y + viewport.Height / 2;
        // Search outwards from the viewport center so consecutive insertions stay visible.
        int centerX = glyph.ScaledX, centerY = glyph.ScaledY;
        int dx = glyph.ScaledWidth + 8, dy = glyph.ScaledHeight + 8;
        var occupied = sheet.Glyphs.OfType<BlockGlyph>().Select(block => block.ScaledBounds).ToArray();
        for (int ring = 1; occupied.Any(bounds => bounds.IntersectsWith(glyph.ScaledBounds)); ring++)
        {
            bool found = false;
            for (int y = -ring; y <= ring && !found; y++)
                for (int x = -ring; x <= ring && !found; x++)
                {
                    if (Math.Abs(x) != ring && Math.Abs(y) != ring) continue;
                    glyph.ScaledX = centerX + x * dx;
                    glyph.ScaledY = centerY + y * dy;
                    found = !occupied.Any(bounds => bounds.IntersectsWith(glyph.ScaledBounds));
                }
        }
        sheet.DeselectAllGlyphs();
        glyph.Selected = true;
        sheet.Add(glyph);
        Core.Instance.CurrentGlyphType = null;

        sheet.ShowProperties();
        Core.Instance.CommitChange();
        Core.Instance.UpdateCommands();
        Core.Instance.Redraw();
    }
}
