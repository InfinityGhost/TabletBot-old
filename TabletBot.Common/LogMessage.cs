using System;
using System.Linq;

namespace TabletBot.Common
{
    public class LogMessage
    {
        public LogMessage(
            string group,
            string message,
            LogLevel level
        )
        {
            Group = group;
            Message = message;
            Level = level;
        }

        public string Group { get; }
        public string Message { get; }
        public LogLevel Level { get; }
        public DateTime Time { get; } = DateTime.Now;
    }
}