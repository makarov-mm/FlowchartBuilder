using Makarov.FlowchartBuilder;
using Xunit;

namespace FlowchartBuilder.Tests;

public sealed class DocumentFileTests : IDisposable
{
    private readonly DirectoryInfo _directory = Directory.CreateTempSubdirectory();

    public void Dispose() => _directory.Delete(recursive: true);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void InterruptedWriteLeavesTheDestinationAndDirectoryIntact(bool exists)
    {
        string path = Path.Combine(_directory.FullName, "loan.mfb");
        if (exists) File.WriteAllText(path, "previous diagram");

        var error = Assert.Throws<IOException>(() => DocumentFile.Write(path, output =>
        {
            output.Write("partial diagram"u8);
            output.Flush();
            throw new IOException("Simulated write failure.");
        }));

        Assert.Equal("Simulated write failure.", error.Message);
        Assert.Equal(exists, File.Exists(path));
        if (exists) Assert.Equal("previous diagram", File.ReadAllText(path));
        Assert.Equal(exists ? new[] { path } : Array.Empty<string>(), Directory.GetFiles(_directory.FullName));
    }

    [Fact]
    public void SavingToANewAndExistingPathProducesTheCompleteLatestFile()
    {
        string path = Path.Combine(_directory.FullName, "loan.mfb");
        DocumentFile.Write(path, output => output.Write("first and longer diagram"u8));
        DocumentFile.Write(path, output => output.Write("second"u8));
        Assert.Equal("second", File.ReadAllText(path));
        Assert.Equal(path, Assert.Single(Directory.GetFiles(_directory.FullName)));
    }
}
