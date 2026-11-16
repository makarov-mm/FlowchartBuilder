namespace Makarov.FlowchartBuilder;

/// <summary>
/// Undo and redo stacks of a document. Every step is a complete snapshot of the document (its XML), which keeps
/// undo exact for any change without per-operation inverse logic; documents are small, so this is cheap.
/// </summary>
public sealed class DocumentHistory(string initialSnapshot)
{
    private const int Capacity = 100;

    private readonly LinkedList<string> _undo = [];
    private readonly Stack<string> _redo = [];

    /// <summary>Snapshot of the current state of the document.</summary>
    public string Current { get; private set; } = initialSnapshot;

    public bool CanUndo => _undo.Count > 0;

    public bool CanRedo => _redo.Count > 0;

    /// <summary>Makes <paramref name="snapshot"/> the current state if it differs; returns whether a step was recorded.</summary>
    public bool Commit(string snapshot)
    {
        if (snapshot == Current)
            return false;

        _undo.AddLast(Current);
        if (_undo.Count > Capacity)
            _undo.RemoveFirst();

        _redo.Clear();
        Current = snapshot;
        return true;
    }

    /// <summary>Steps back; returns the snapshot to restore, or null when there is nothing to undo.</summary>
    public string? Undo()
    {
        if (_undo.Last is not { } previous)
            return null;

        _undo.RemoveLast();
        _redo.Push(Current);
        Current = previous.Value;
        return Current;
    }

    /// <summary>Steps forward; returns the snapshot to restore, or null when there is nothing to redo.</summary>
    public string? Redo()
    {
        if (!_redo.TryPop(out string? next))
            return null;

        _undo.AddLast(Current);
        Current = next;
        return Current;
    }
}
