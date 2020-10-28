using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TabletBot.Common;

namespace TabletBot
{
    internal class BotCommandDictionary : IEnumerable<BotCommand>
    {
        private IList<BotCommand> Commands { set; get; } = new List<BotCommand>();

        public BotCommand this[string name]
        {
            get
            {
                return Commands.FirstOrDefault(c => c.Name == name);
            }
            set
            {
                if (Commands.FirstOrDefault(c => c.Name == name) is BotCommand cmd)
                {
                    var index = Commands.IndexOf(cmd);
                    Commands[index] = value;
                }
                else
                {
                    Commands.Add(value);
                }
            }
        }

        public void Invoke(string full)
        {
            var arguments = full.Split(' ', 2);
            var name = arguments[0];
            var args = arguments.Length > 1 ? arguments[1] : null;
            Invoke(name, args);
        }

        public void Invoke(string name, string args)
        {
            if (this[name] is BotCommand cmd)
                cmd.Invoke(args);
            else
                Log.Write("Error", "Invalid command.", LogLevel.Error);
        }

        public void Add(BotCommand command)
        {
            Commands.Add(command);
        }

        public IEnumerator<BotCommand> GetEnumerator()
        {
            return Commands.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Commands.GetEnumerator();
        }
    }
}