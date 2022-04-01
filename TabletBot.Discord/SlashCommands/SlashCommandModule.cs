using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace TabletBot.Discord.SlashCommands
{
    public abstract class SlashCommandModule
    {
        public event Func<SlashCommandModule, Task>? Update;

        public void OnUpdate() => Update?.Invoke(this);

        public async Task HandleInteraction(SocketInteraction interaction)
        {
            if (interaction is SocketSlashCommand slashCommand)
            {
                if (CommandHandlers.FirstOrDefault(c => c.Name == slashCommand.Data.Name) is SlashCommand command)
                {
                    await command.Invoke(slashCommand);
                }
            }
        }

        public IList<SlashCommand> CommandHandlers { set; get; } = Array.Empty<SlashCommand>();

        protected abstract IEnumerable<SlashCommand> GetSlashCommands();

        public IList<SlashCommand> BuildCommandHandlers()
        {
            return CommandHandlers = new List<SlashCommand>(GetSlashCommands());
        }
    }
}