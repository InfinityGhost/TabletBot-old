using System;
using JetBrains.Annotations;

namespace TabletBot.Common.Attributes.Bot
{
    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse(ImplicitUseTargetFlags.Members)]
    public class CommandAttribute : Attribute
    {
        public CommandAttribute(params string[] arguments)
        {
            Arguments = arguments;
        }

        public string[] Arguments { get; }
    }
}