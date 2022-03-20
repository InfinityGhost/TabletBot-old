using System;

namespace TabletBot
{
    public class Box : IDisposable
    {
        public Box(string header = null)
        {
            var headerText = string.IsNullOrWhiteSpace(header) ? string.Empty : $" {header} ";
            IO.WriteLine(
                IO.BOX_VERTICAL_RIGHT + headerText +
                IO.Repeat(IO.BOX_HORIZONTAL, Console.WindowWidth - 2 - headerText.Length) +
                IO.BOX_VERTICAL_LEFT
            );
        }

        public void Dispose()
        {
            IO.WriteLine(
                IO.BOX_VERTICAL_RIGHT +
                IO.Repeat(IO.BOX_HORIZONTAL, Console.WindowWidth - 2) +
                IO.BOX_VERTICAL_LEFT
            );
        }

        private static int MaxLineLength = Console.WindowWidth - 4;

        public void WriteLine(string text)
        {
            foreach (var line in IO.Split(text, MaxLineLength))
            {
                var padded = string.Format($"{{0,-{MaxLineLength}}}", line);
                var formatted = $"{IO.BOX_VERTICAL} {padded} {IO.BOX_VERTICAL}";
                IO.WriteLine(formatted);
            }
        }
    }
}