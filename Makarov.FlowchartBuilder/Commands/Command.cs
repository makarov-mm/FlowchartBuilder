namespace Makarov.FlowchartBuilder.Commands;

/// <summary>
/// An action invoked from the menus. Commands are created on first use by class name (as written in the
/// menu definition files) and shared by all menu items and toolbar buttons bound to them.
/// </summary>
public abstract class Command
{
    private readonly List<ToolStripItem> _items = [];

    /// <summary>Enabled state of the bound menu items and toolbar buttons.</summary>
    public bool Enabled
    {
        get => _items.Count > 0 && _items[0].Enabled;
        set
        {
            foreach (ToolStripItem item in _items)
                item.Enabled = value;
        }
    }

    /// <summary>Checked state of the bound menu items and toolbar buttons.</summary>
    public bool Checked
    {
        get => _items.Count > 0 && IsChecked(_items[0]);
        set
        {
            foreach (ToolStripItem item in _items)
                SetChecked(item, value);
        }
    }

    /// <exception cref="KeyNotFoundException">There is no command class with that name.</exception>
    public static Command GetInstance(string name) => Core.Instance.CommandFor(name);

    internal static Command Create(string name)
    {
        Type? type = Type.GetType($"{typeof(Command).Namespace}.{name}");
        if (type is null || type.IsAbstract || !type.IsSubclassOf(typeof(Command)))
            throw new KeyNotFoundException($"Command '{name}' does not exist.");

        return (Command)Activator.CreateInstance(type)!;
    }

    public static TCommand GetInstance<TCommand>()
        where TCommand : Command =>
        (TCommand)GetInstance(typeof(TCommand).Name);

    public abstract void Run();

    /// <summary>Runs the command and records the change it made to the document, if any, as one undo step.</summary>
    public void Execute()
    {
        Run();
        Core.Instance.CommitChange();
    }

    /// <summary>Binds a menu item or toolbar button so that it follows <see cref="Enabled"/> and <see cref="Checked"/>.</summary>
    public void Attach(ToolStripItem item)
    {
        if (!_items.Contains(item))
            _items.Add(item);
    }

    private static bool IsChecked(ToolStripItem item) => item switch
    {
        ToolStripMenuItem menuItem => menuItem.Checked,
        ToolStripButton button => button.Checked,
        _ => false,
    };

    private static void SetChecked(ToolStripItem item, bool value)
    {
        switch (item)
        {
            case ToolStripMenuItem menuItem:
                menuItem.Checked = value;
                break;

            case ToolStripButton button:
                button.Checked = value;
                break;
        }
    }
}
