using System;
using System.Collections.Generic;
using System.Reflection;
using TabletBot.Common.Attributes.Bot;

namespace TabletBot
{
    public class Command
    {
        public Command(MethodInfo method, CommandAttribute attr)
        {
            this.Method = method;
            this.Arguments = attr.Arguments;
        }

        public MethodInfo Method { get; }
        public IReadOnlyList<string> Arguments { get; }

        public void Invoke(string[] args)
        {
            if (args.Length != Arguments.Count)
            {
                throw new ArgumentException("Invalid number of arguments.");
            }

            Method.Invoke(null, new object[] { args });
        }
    }
}