using System.Globalization;

namespace Makarov.FlowchartBuilder.Commands;

public sealed class ZoomCommand : CommandWithSubcommands
{
    private static readonly int[] Percents = [400, 200, 150, 125, 100, 75, 50, 25];

    public override IEnumerable<string> Commands => Percents.Select(percent => $"{percent}%");

    public override void Run(string subcommand)
    {
        if (Core.Instance.ActiveSheet is not { } sheet)
            return;

        sheet.ScaleFactor = int.Parse(subcommand.TrimEnd('%'), CultureInfo.InvariantCulture) / 100f;
        Core.Instance.Redraw();
    }
}
