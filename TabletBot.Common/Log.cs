using System;

namespace TabletBot.Common
{
    public static class Log
    {
        public static event Action<LogMessage>? Output;

        public static void Write(string group, string text, LogLevel level = LogLevel.Info)
        {
            Output?.Invoke(new LogMessage(group, text, level));
        }

        public static void Debug(string group, string text)
        {
            Write(group, text, LogLevel.Debug);
        }

        public static void Exception(Exception exception)
        {
            Output?.Invoke(new ExceptionLogMessage(exception));
        }
    }
}