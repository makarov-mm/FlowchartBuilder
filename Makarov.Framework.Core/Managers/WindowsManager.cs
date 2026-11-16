namespace Makarov.Framework.Core.Managers;

/// <summary>
/// Keeps one live instance per form type and recreates it after it has been disposed.
/// </summary>
public sealed class WindowsManager : IDisposable
{
    private readonly Dictionary<Type, Form> _windows = [];
    private bool _disposed;

    public IEnumerable<Form> Windows
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _windows.Values.Where(window => !window.IsDisposed).ToList();
        }
    }

    public T GetWindow<T>()
        where T : Form, new()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_windows.TryGetValue(typeof(T), out Form? window) && !window.IsDisposed)
            return (T)window;

        var newWindow = new T();
        _windows[typeof(T)] = newWindow;
        return newWindow;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        foreach (Form window in _windows.Values)
            window.Dispose();

        _windows.Clear();
        _disposed = true;
    }
}
