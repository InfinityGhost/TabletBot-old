using System;
using System.Threading.Tasks;
using Message = TabletBot.Common.LogMessage;

namespace TabletBot.Common
{
    public static class Log
    {
        public static event EventHandler<Message> Output;

        public static void Write(string group, string text)
        {
            Output?.Invoke(null, new Message(group, text));
        }

        public static async Task WriteAsync(string group, string text) => await Task.Run(() => Write(group, text));

        public static void Debug(string text)
        {
            Write("DEBUG", text);
        }

        public static void Exception(Exception exception)
        {
            Write(exception.GetType().Name, exception.Message);
        }
    }
}