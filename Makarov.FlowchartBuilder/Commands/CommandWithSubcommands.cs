namespace Makarov.FlowchartBuilder.Commands;

/// <summary>
/// A command shown as a submenu with one item per subcommand (for example, one per language).
/// </summary>
public abstract class CommandWithSubcommands : Command
{
    /// <summary>Names of the subcommands; they are also translation keys for the item captions.</summary>
    public abstract IEnumerable<string> Commands { get; }

    /// <summary>The submenu itself does nothing.</summary>
    public override void Run()
    {
    }

    public abstract void Run(string subcommand);

    /// <summary>Icon of a subcommand item; the caller owns the bitmap.</summary>
    public virtual Bitmap? GetIcon(string subcommand) => null;
}
