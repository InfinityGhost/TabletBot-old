using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TabletBot
{
    internal static class Output
    {
        public static void WriteDivider()
        {
            var chars = Enumerable.Repeat('-', System.Console.BufferWidth);
            var line = string.Concat(chars);
            WriteLine(line);
        }

        public static async void WriteLine(string text)
        {
            var index = Platform.IsLinux ? 1 : System.Console.CursorLeft;
            var chars = Enumerable.Repeat('\b', index);
            await System.Console.Out.WriteAsync(string.Concat(chars));
            await System.Console.Out.WriteLineAsync(text);
            Flush();
        }

        public static async void Flush()
        {
            await System.Console.Out.WriteAsync('>');
        }
    }
}