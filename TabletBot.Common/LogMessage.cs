using System;

namespace TabletBot.Common
{
    public class LogMessage
    {
        public readonly string Group;
        public readonly string Text;
        public readonly DateTime Time = DateTime.Now;

        public LogMessage(string group, string text)
        {
            Group = group;
            Text = text;
        }
    }
}