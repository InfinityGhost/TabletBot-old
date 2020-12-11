using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TabletBot.Common.Attributes.Bot;

namespace TabletBot
{
    public class CommandCollection : IReadOnlyList<Command>
    {
        protected CommandCollection()
        {
            var query = from method in typeof(Program).GetMethods()
                where method.IsStatic
                let attr = method.GetCustomAttribute<CommandAttribute>()
                where attr is CommandAttribute
                select new Command(method, attr);
            this.collection = query.ToList();
        }

        protected IList<Command> collection;

        public static CommandCollection Current { get; } = new CommandCollection();

        public bool Invoke(string[] args)
        {
            var commandName = args[0];
            var commandArgs = args.Length > 1 ? args[1..^0] : new string[0];

            if (collection.FirstOrDefault(c => MatchCommand(c, commandName)) is Command command)
            {
                command.Invoke(commandArgs);
                return true;
            }

            return false;
        }

        private bool MatchCommand(Command command, string name)
        {
            var commandName = name.ToLower();
            var methodName = command.Method.Name.ToLower();
            var aliases = command.Method.GetCustomAttributes<AliasAttribute>();

            return methodName == commandName || aliases.Any(a => a.Alias.ToLower() == commandName);
        }

        public Command this[int index] => this.collection[index];

        public int Count => this.collection.Count;

        public IEnumerator<Command> GetEnumerator()
        {
            return this.collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this.collection as IEnumerable).GetEnumerator();
        }
    }
}