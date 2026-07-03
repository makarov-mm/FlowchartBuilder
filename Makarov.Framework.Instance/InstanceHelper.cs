using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace Makarov.Framework.Instance
{
    public static class InstanceHelper
    {
        private static Mutex _mutex;
        private static CancellationTokenSource _cts;
        private static Thread _listener;

        public static bool TryStartPrimary(string name, IInstance handler)
        {
            _mutex = new Mutex(true, MutexName(name), out bool createdNew);
            if (!createdNew)
            {
                _mutex.Dispose();
                _mutex = null;
                return false;
            }

            _cts = new CancellationTokenSource();
            _listener = new Thread(() => ListenLoop(name, handler, _cts.Token))
            {
                IsBackground = true,
                Name = "SingleInstanceListener"
            };
            _listener.Start();
            return true;
        }

        public static void SendToPrimary(string name, string[] messages)
        {
            using var client = new NamedPipeClientStream(".", PipeName(name), PipeDirection.Out);
            client.Connect(3000);
            using var writer = new BinaryWriter(client, Encoding.UTF8, leaveOpen: true);
            writer.Write(messages?.Length ?? 0);
            if (messages != null)
                foreach (var message in messages)
                    writer.Write(message);
            writer.Flush();
        }

        public static void Stop()
        {
            _cts?.Cancel();
            try
            {
                using var wake = new NamedPipeClientStream(".", PipeName_Internal(), PipeDirection.Out);
            }
            catch
            {
            }
            _mutex?.Dispose();
            _mutex = null;
        }

        private static string _lastName;

        private static void ListenLoop(string name, IInstance handler, CancellationToken token)
        {
            _lastName = name;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using var server = new NamedPipeServerStream(
                        PipeName(name), PipeDirection.In, 1,
                        PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                    server.WaitForConnection();

                    if (token.IsCancellationRequested)
                        break;

                    using var reader = new BinaryReader(server, Encoding.UTF8, leaveOpen: true);
                    int count = reader.ReadInt32();
                    var messages = new string[Math.Max(0, count)];
                    for (int i = 0; i < messages.Length; i++)
                        messages[i] = reader.ReadString();

                    handler.Message(messages);
                }
                catch (IOException)
                {
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }

        private static string PipeName_Internal() => PipeName(_lastName);

        private static string PipeName(string name) => $"{name}-{Environment.UserName}-pipe";

        private static string MutexName(string name) => $"Local\\{name}-{Environment.UserName}-mutex";
    }
}
