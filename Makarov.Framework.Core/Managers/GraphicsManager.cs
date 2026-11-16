namespace Makarov.Framework.Core.Managers;

/// <summary>
/// Loads images by name (without extension) from a folder and caches them for the lifetime of the manager.
/// Images are copied into memory, so the files are not kept locked.
/// </summary>
public sealed class GraphicsManager : IDisposable
{
    private static readonly string[] Extensions = [".bmp", ".png", ".jpg"];

    private readonly Dictionary<string, Bitmap> _images = new(StringComparer.OrdinalIgnoreCase);

    public GraphicsManager(string directory)
    {
        if (!System.IO.Directory.Exists(directory))
            throw new DirectoryNotFoundException($"Directory '{directory}' not found.");

        Directory = directory;
    }

    public string Directory { get; }

    /// <summary>Returns the cached image. It is shared: callers must neither modify nor dispose it.</summary>
    public Bitmap this[string name]
    {
        get
        {
            if (!_images.TryGetValue(name, out Bitmap? image))
            {
                image = Load(name);
                _images.Add(name, image);
            }

            return image;
        }
    }

    /// <summary>Returns a private copy of the image that the caller owns.</summary>
    public Bitmap GetCopy(string name) => new(this[name]);

    /// <summary>Like <see cref="GetCopy"/>, but returns <see langword="null"/> when there is no such image.</summary>
    public Bitmap? TryGetCopy(string name) => Exists(name) ? GetCopy(name) : null;

    public bool Exists(string name) =>
        _images.ContainsKey(name) || Extensions.Any(extension => File.Exists(Path.Combine(Directory, name + extension)));

    public void Dispose()
    {
        foreach (Bitmap image in _images.Values)
            image.Dispose();

        _images.Clear();
    }

    private Bitmap Load(string name)
    {
        foreach (string extension in Extensions)
        {
            string fileName = Path.Combine(Directory, name + extension);
            if (!File.Exists(fileName))
                continue;

            using var stream = File.OpenRead(fileName);
            using var image = Image.FromStream(stream);
            return new Bitmap(image);
        }

        throw new FileNotFoundException($"Image '{name}' not found in '{Directory}'.");
    }
}
