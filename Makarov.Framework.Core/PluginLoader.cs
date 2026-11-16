using System.Reflection;
using System.Runtime.Loader;

namespace Makarov.Framework.Core;

/// <summary>
/// Loads every managed assembly from a plugins folder into the default load context, so plugins share
/// the host's copies of the framework assemblies. Files that are not loadable plugins are skipped.
/// </summary>
public sealed class PluginLoader
{
    private readonly List<Assembly> _assemblies = [];
    private List<Type>? _types;

    public PluginLoader(string directory)
    {
        if (!Directory.Exists(directory))
            return;

        foreach (string file in Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories))
        {
            try
            {
                _assemblies.Add(AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(file)));
            }
            catch (Exception e) when (e is BadImageFormatException or FileLoadException)
            {
                // Native library, or a stray copy of an assembly the host has already loaded.
            }
        }
    }

    public IReadOnlyList<Assembly> Assemblies => _assemblies;

    public IReadOnlyList<Type> Types => _types ??= [.. _assemblies.SelectMany(GetLoadableTypes)];

    public Type? FindType(string fullName) => Types.FirstOrDefault(type => type.FullName == fullName);

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.OfType<Type>();
        }
    }
}
