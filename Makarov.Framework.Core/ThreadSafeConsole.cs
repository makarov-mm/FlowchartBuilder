using System;

namespace Makarov.Framework.Core
{

    public static class ThreadSafeConsole
    {
        private static readonly object SyncObject = new object();

        public static void Write(string message)
        {
            lock (SyncObject)
            {
                Console.Write(message);
            }
        }

        public static void Write(string message, ConsoleColor color)
        {
            lock (SyncObject)
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.Write(message);
                Console.ForegroundColor = oldColor;
            }
        }

        public static void WriteLine(string message)
        {
            lock (SyncObject)
            {
                Console.WriteLine(message);
            }
        }

        public static void WriteLine(string message, ConsoleColor color)
        {
            lock (SyncObject)
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ForegroundColor = oldColor;
            }
        }

        public static void Write(string format, params string[] args)
        {
            lock (SyncObject)
            {
                Console.Write(format, args);
            }
        }

        public static void Write(string format, ConsoleColor color, params string[] args)
        {
            lock (SyncObject)
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.Write(format, args);
                Console.ForegroundColor = oldColor;
            }
        }

        public static void WriteLine(string format, params string[] args)
        {
            lock (SyncObject)
            {
                Console.WriteLine(format, args);
            }
        }

        public static void WriteLine(string format, ConsoleColor color, params string[] args)
        {
            lock (SyncObject)
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(format, args);
                Console.ForegroundColor = oldColor;
            }
        }
    }
}
