using System.IO.Pipes;
using System.Text;

namespace Makarov.Framework.Instance;

/// <summary>
/// Single-instance guard: a named <see cref="Mutex"/> detects the primary instance and a named pipe
/// lets secondary instances hand their command-line arguments over to it.
/// </summary>
public sealed class SingleInstance : IDisposable
{
    private const int ConnectTimeoutMilliseconds = 3000;
    private const int MaxMessageCount = 1024;

    private readonly string _pipeName;
    private readonly Mutex _mutex;
    private readonly CancellationTokenSource _cancellation = new();
    private Task? _listener;

    private SingleInstance(string name, Mutex mutex)
    {
        _pipeName = PipeName(name);
        _mutex = mutex;
    }

    /// <summary>Returns the guard when this process is the primary instance, otherwise <see langword="null"/>.</summary>
    public static SingleInstance? TryAcquire(string name)
    {
        var mutex = new Mutex(initiallyOwned: true, MutexName(name), out bool createdNew);
        if (createdNew)
            return new SingleInstance(name, mutex);

        mutex.Dispose();
        return null;
    }

    /// <summary>Forwards <paramref name="messages"/> to the primary instance. Returns <see langword="false"/> if it is not reachable.</summary>
    public static bool TrySendToPrimary(string name, IReadOnlyCollection<string> messages)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName(name), PipeDirection.Out, PipeOptions.CurrentUserOnly);
            client.Connect(ConnectTimeoutMilliseconds);

            using var writer = new BinaryWriter(client, Encoding.UTF8);
            writer.Write(messages.Count);
            foreach (string message in messages)
                writer.Write(message);

            return true;
        }
        catch (Exception e) when (e is IOException or TimeoutException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    /// <summary>Starts accepting messages from secondary instances. Subsequent calls are ignored.</summary>
    public void StartListening(IInstance handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _listener ??= Task.Run(() => ListenAsync(handler, _cancellation.Token));
    }

    public void Dispose()
    {
        _cancellation.Cancel();

        try
        {
            _listener?.Wait(TimeSpan.FromSeconds(1));
        }
        catch (AggregateException)
        {
        }

        _cancellation.Dispose();
        _mutex.Dispose();
    }

    private async Task ListenAsync(IInstance handler, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await using var server = new NamedPipeServerStream(
                    _pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

                await server.WaitForConnectionAsync(token).ConfigureAwait(false);
                handler.Message(ReadMessages(server));
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (IOException)
            {
                // A client disconnected early or sent a malformed message: keep listening.
            }
        }
    }

    private static string[] ReadMessages(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        int count = reader.ReadInt32();
        if (count is < 0 or > MaxMessageCount)
            throw new IOException($"Invalid message count: {count}.");

        var messages = new string[count];
        for (int i = 0; i < count; i++)
            messages[i] = reader.ReadString();

        return messages;
    }

    private static string PipeName(string name) => $"{name}-{Environment.UserName}-pipe";

    private static string MutexName(string name) => $@"Local\{name}-{Environment.UserName}-mutex";
}
