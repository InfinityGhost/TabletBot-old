using System;
using System.Collections.Generic;
using System.Linq;

namespace TabletBot
{
    public class Box : IDisposable
    {
        public Box(string header = null)
        {
            var headerText = string.IsNullOrWhiteSpace(header) ? string.Empty : $" {header} ";
            IO.WriteLine(
                IO.BOX_VERTICAL_RIGHT + headerText +
                string.Concat(Enumerable.Repeat(IO.BOX_HORIZONTAL, Console.WindowWidth - 2 - headerText.Length)) +
                IO.BOX_VERTICAL_LEFT
            );
        }

        public void Dispose()
        {
            IO.WriteLine(
                IO.BOX_VERTICAL_RIGHT +
                string.Concat(Enumerable.Repeat(IO.BOX_HORIZONTAL, Console.WindowWidth - 2)) +
                IO.BOX_VERTICAL_LEFT
            );
        }

        private static int MaxLineLength = Console.WindowWidth - 4;

        public void WriteLine(string text)
        {
            foreach (var line in Split(text))
            {
                var padded = string.Format($"{{0,-{MaxLineLength}}}", line);
                var formatted = $"{IO.BOX_VERTICAL} {padded} {IO.BOX_VERTICAL}";
                IO.WriteLine(formatted);
            }
        }

        public static IEnumerable<string> Split(string str)
        {
            foreach (var line in str.Split(Environment.NewLine))
            {
                int index = 0;
                int length = 0;
                while (index < line.Length)
                {
                    length = Math.Clamp(MaxLineLength, 0, line.Length - index);
                    yield return line.Substring(index, length);
                    index += length;
                }
            }
        }
    }
}