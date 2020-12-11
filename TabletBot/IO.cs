using System;
using System.Collections.Generic;
using System.Linq;
using TabletBot.Common;

namespace TabletBot
{
    internal static class IO
    {
        private const char BACKSPACE_CHAR = '\b';
        public const char BOX_VERTICAL = '│';
        public const char BOX_HORIZONTAL = '─';
        public const char BOX_TOP_LEFT = '┌';
        public const char BOX_TOP_RIGHT = '┐';
        public const char BOX_BOTTOM_LEFT = '└';
        public const char BOX_BOTTOM_RIGHT = '┘';
        public const char BOX_VERTICAL_RIGHT = '├';
        public const char BOX_VERTICAL_LEFT = '┤';
        public const char BOX_HORIZONTAL_UP = '┴';
        public const char BOX_HORIZONTAL_DOWN = '┬';
        public const char BOX_CROSS = '┼';

        private static IList<string> CommandHistory { set; get; } = new List<string>();
        private static int HistoryPositionIndex { set; get; } = 0;

        private static string CurrentInputBuffer { set; get; } = string.Empty;
        private static string CurrentHistory { set; get; } = string.Empty;

        private static string InputBufferText => $"╘═> {CurrentInputBuffer}";

        public static string ReadLine()
        {
            while (Console.ReadKey() is ConsoleKeyInfo keyInfo)
            {
                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter:
                    {
                        var buffer = CurrentInputBuffer;
                        CommandHistory.Add(buffer);

                        CurrentInputBuffer = string.Empty;
                        HistoryPositionIndex = 0;
                        return buffer;
                    }
                    case ConsoleKey.Backspace:
                    {
                        if (CurrentInputBuffer.Length > 0)
                        {
                            Console.Out.Write($"{BACKSPACE_CHAR} {BACKSPACE_CHAR}");
                            CurrentInputBuffer = CurrentInputBuffer[0..^1];
                        }
                        break;
                    }
                    case ConsoleKey.UpArrow:
                    {
                        if (HistoryPositionIndex < CommandHistory.Count)
                        {
                            if (HistoryPositionIndex == 0)
                                CurrentHistory = CurrentInputBuffer;

                            HistoryPositionIndex++;
                            UpdateBuffer();
                        }
                        break;
                    }
                    case ConsoleKey.DownArrow:
                    {
                        if (HistoryPositionIndex > 0)
                        {
                            HistoryPositionIndex--;
                            UpdateBuffer();
                        }
                        break;
                    }
                    default:
                    {
                        // Limit to only ASCII characters
                        if (keyInfo.KeyChar < 128)
                        {
                            CurrentInputBuffer += keyInfo.KeyChar;
                        }
                        break;
                    }
                }
            }
            return null;
        }

        public static void WriteLine(string text)
        {
            ClearLine();
            Console.Out.WriteLine(text);
            WriteBufferPrefix();
        }

        public static void ClearLine()
        {
            var delStr = Repeat($"{BACKSPACE_CHAR} {BACKSPACE_CHAR}", Console.WindowWidth);
            Console.Write(delStr);
        }

        public static void WriteMessageHeader()
        {
            WriteLine(
                BOX_TOP_LEFT + Repeat(BOX_HORIZONTAL, 13) +
                BOX_HORIZONTAL_DOWN + Repeat(BOX_HORIZONTAL, 9) +
                BOX_HORIZONTAL_DOWN + Repeat(BOX_HORIZONTAL, 12) +
                BOX_HORIZONTAL_DOWN + Repeat(BOX_HORIZONTAL, Console.WindowWidth - 39) +
                BOX_TOP_RIGHT
            );
            WriteLine(
                $"{BOX_VERTICAL} " +
                $"{nameof(LogMessage.Time),-11} {BOX_VERTICAL} " +
                $"{nameof(LogMessage.Level),-7} {BOX_VERTICAL} " +
                $"{nameof(LogMessage.Group),-10} {BOX_VERTICAL} " +
                string.Format($"{{0,-{Console.WindowWidth - 40}}}", nameof(LogMessage.Message)) +
                BOX_VERTICAL
            );
            WriteLine(
                BOX_VERTICAL_RIGHT + Repeat(BOX_HORIZONTAL, 13) +
                BOX_CROSS + Repeat(BOX_HORIZONTAL, 9) +
                BOX_CROSS + Repeat(BOX_HORIZONTAL, 12) +
                BOX_CROSS + Repeat(BOX_HORIZONTAL, Console.WindowWidth - 39) +
                BOX_VERTICAL_LEFT
            );
        }

        public static void WriteLine(LogMessage message)
        {
            if (message is ExceptionLogMessage exceptionLogMessage)
            {
                using (var box = new Box(exceptionLogMessage.Exception.GetType().FullName))
                    box.WriteLine(exceptionLogMessage.Exception.StackTrace);
            }
            else
            {
                WriteLine(
                    $"{BOX_VERTICAL} " +
                    $"{Clamp(message.Time.ToLongTimeString(), 11)} {BOX_VERTICAL} " +
                    $"{Clamp(message.Level, 7)} {BOX_VERTICAL} " +
                    $"{Clamp(message.Group, 10)} {BOX_VERTICAL} " +
                    string.Format($"{{0,-{Console.WindowWidth - 40}}}", message.Message.Trim()) + BOX_VERTICAL
                );
            }
        }


        private static void UpdateBuffer()
        {
            ClearLine();
            CurrentInputBuffer = HistoryPositionIndex == 0 ? CurrentHistory : CommandHistory[CommandHistory.Count - HistoryPositionIndex];
            WriteBufferPrefix();
        }

        private static void WriteBufferPrefix()
        {
            Console.Out.Write(InputBufferText);
        }

        private static string Clamp(object val, int length)
        {
            string str = val as string ?? val.ToString();
            if (str.Length > length)
                return str.Substring(0, length);
            else
                return string.Format($"{{0,-{length}}}", str);
        }

        private static string Repeat<T>(T value, int count) => string.Concat(Enumerable.Repeat(value, count));
    }
}