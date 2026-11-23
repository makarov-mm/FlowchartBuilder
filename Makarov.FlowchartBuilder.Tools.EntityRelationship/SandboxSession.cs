namespace Makarov.FlowchartBuilder.Tools.EntityRelationship;

internal sealed class SandboxSession : IDisposable
{
    private (string Sql, bool Library)? _requested;
    private (string Sql, bool Library)? _active;
    private SandboxDatabase? _database;

    public bool HasSchema => _requested.HasValue;
    public bool HasDatabase => _database != null;
    public bool CanRun => HasDatabase && _requested == _active;
    public bool HasLibraryData { get; private set; }
    public IEnumerable<string> TableNames => _database?.TableNames ?? [];
    public SandboxDatabase ActiveDatabase => CanRun ? _database! :
        throw new InvalidOperationException("The test database uses a different schema.");

    public bool SetSchema(string? sql, bool library)
    {
        (string Sql, bool Library)? next = sql == null ? null : (sql, library);
        if (next == _requested) return false;
        _requested = next;
        return true;
    }

    // Prepare the replacement before releasing the current database. Undo can reactivate it after a failed rebuild.
    public string? Rebuild()
    {
        if (_requested is not { } schema) throw new InvalidOperationException("Select a valid schema first.");
        var replacement = new SandboxDatabase(schema.Sql);
        string? seedError = null;
        try
        {
            if (schema.Library)
            {
                try { replacement.Execute(SandboxExamples.Seed); }
                // The edited library may require an extra column. The seed batch rolls back and the empty schema is usable.
                catch (InvalidOperationException error) { seedError = error.Message; }
            }
        }
        catch
        {
            replacement.Dispose();
            throw;
        }

        _database?.Dispose();
        _database = replacement;
        _active = schema;
        HasLibraryData = schema.Library && seedError == null;
        return seedError;
    }

    public SandboxResult Execute(string query) => ActiveDatabase.Execute(query);

    public void Dispose()
    {
        _database?.Dispose();
        _database = null;
    }
}
