namespace Makarov.FlowchartBuilder;

internal static class DocumentFile
{
    public static void Write(string fileName, Action<Stream> write)
    {
        var file = new FileInfo(fileName);
        // Saving through a symbolic link should update the linked document.
        string destination = file.FullName;
        if (file.LinkTarget != null && file.ResolveLinkTarget(returnFinalTarget: true) is { } target)
            destination = target.FullName;
        string temporary = Path.Combine(Path.GetDirectoryName(destination)!, Path.GetRandomFileName());
        var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        try
        {
            using (stream)
            {
                write(stream);
                stream.Flush(flushToDisk: true);
            }
            if (File.Exists(destination))
                File.Replace(temporary, destination, null);
            else
                File.Move(temporary, destination);
        }
        catch
        {
            // Report the original save failure even if cleanup is also denied.
            try { File.Delete(temporary); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            throw;
        }
    }
}
