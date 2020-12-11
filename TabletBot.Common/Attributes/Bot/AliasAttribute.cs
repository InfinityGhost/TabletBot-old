using System;

namespace TabletBot.Common.Attributes.Bot
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class AliasAttribute : Attribute
    {
        public AliasAttribute(string alias)
        {
            Alias = alias;
        }

        public string Alias { get; }
    }
}