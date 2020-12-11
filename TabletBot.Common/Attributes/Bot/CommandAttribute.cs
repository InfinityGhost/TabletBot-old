using System;

namespace TabletBot.Common.Attributes.Bot
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public CommandAttribute(params string[] arguments)
        {
            Arguments = arguments;
        }

        public string[] Arguments { get; }
    }
}