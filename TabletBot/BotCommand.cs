using System;
using System.Collections.Generic;
using System.Reflection;
using TabletBot.Discord;

namespace TabletBot
{
    internal class BotCommand
    {
        public BotCommand(string name, CommandDelegate command)
        {
            Name = name;
            Delegate = command;
        }

        public string Name { private set; get; }
        public CommandDelegate Delegate { private set; get; }

        public virtual void Invoke(string args)
        {
            Delegate.DynamicInvoke(args);
        }
    }

    internal delegate void CommandDelegate(string args);
}