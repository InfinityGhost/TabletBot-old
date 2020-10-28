using System;

namespace TabletBot.Common
{
    public class LogMessage
    {
        public readonly string Group;
        public readonly string Text;
        public readonly DateTime Time = DateTime.Now;
        public readonly LogLevel Level;

        public LogMessage(string group, string text, LogLevel level)
        {
            this.Group = group;
            this.Text = text;
            this.Level = level;
        }
    }
}