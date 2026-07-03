using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Makarov.Framework.VersionUpdater
{
    public sealed class Program
    {
        static void Main()
        {

            string currentDirectory = AppContext.BaseDirectory;

            string[] files = Directory.GetFiles(currentDirectory, @"AssemblyInfo.cs", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                Console.WriteLine(@"File '{0}'...", file);

                var regex = new Regex("Version\\(\"[0-9]*\\.[0-9]*\\.[0-9]*\\.[0-9]*\"\\)");
                string text = File.ReadAllText(file);
                MatchCollection matches = regex.Matches(text);

                foreach (Match match in matches)
                {

                    string oldValue = match.ToString();

                    int idx0 = oldValue.LastIndexOf('.');
                    int idx1 = oldValue.LastIndexOf('"');
                    string firstPart = oldValue.Remove(idx0 + 1);
                    string secondPart = oldValue.Remove(0, idx0 + 1).Remove(idx1 - idx0 - 1);
                    string thirdPart = oldValue.Remove(0, idx1);

                    int ver = int.Parse(secondPart);
                    int newVersion = ver + 1;
                    Console.WriteLine("Version changed {0} -> {1}", ver, newVersion);

                    string newValue = firstPart + newVersion + thirdPart;
                    Console.WriteLine("Old: {0}", oldValue);
                    Console.WriteLine("New: {0}", newValue);
                    text = text.Replace(oldValue, newValue);
                }

                File.WriteAllText(file, text);
            }
        }
    }
}
