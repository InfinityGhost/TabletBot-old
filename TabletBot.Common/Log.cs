using System;
using System.Threading.Tasks;
using Message = TabletBot.Common.LogMessage;

namespace TabletBot.Common
{
    public static class Log
    {
        public static event EventHandler<Message> Output;

        public static void Write(string group, string text, LogLevel level)
        {
            Output?.Invoke(null, new Message(group, text, level));
        }

        public static async Task WriteAsync(string group, string text, LogLevel level) => await Task.Run(() => Write(group, text, level));

        public static void Debug(string text)
        {
            Write("DEBUG", text, LogLevel.Debug);
        }

        public static void Exception(Exception exception)
        {
            Write(exception.GetType().Name, exception.Message, LogLevel.Error);
        }
    }
}