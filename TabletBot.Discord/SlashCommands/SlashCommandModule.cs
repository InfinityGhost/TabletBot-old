using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using TabletBot.Common;

namespace TabletBot.Discord.SlashCommands
{
    public abstract class SlashCommandModule
    {
        public virtual async Task Hook(DiscordSocketClient client)
        {
            BuildCommandHandlers();

            foreach (var command in CommandHandlers)
                await client.Rest.CreateGuildCommand(command.Build(), Settings.Current.GuildID);

            client.InteractionCreated += HandleInteraction;
        }

        public virtual async Task Update(DiscordSocketClient client)
        {
            BuildCommandHandlers();
            
            foreach (var command in await client.Rest.GetGuildApplicationCommands(Settings.Current.GuildID))
                await command.DeleteAsync();

            foreach (var command in CommandHandlers)
                await client.Rest.CreateGuildCommand(command.Build(), Settings.Current.GuildID);
        }

        public virtual async Task HandleInteraction(SocketInteraction interaction)
        {
            if (interaction is SocketSlashCommand slashCommand)
            {
                if (CommandHandlers.FirstOrDefault(c => c.Name == slashCommand.Data.Name) is SlashCommand command)
                {
                    await command.Invoke(slashCommand);
                }
            }
        }

        protected IList<SlashCommand> CommandHandlers { set; get; }
        
        protected abstract IEnumerable<SlashCommand> GetSlashCommands();
        
        protected virtual void BuildCommandHandlers()
        {
            CommandHandlers = new List<SlashCommand>(GetSlashCommands());
        }

        protected static T GetValue<T>(SocketSlashCommand command, string option)
        {
            var value = command.Data.Options.FirstOrDefault(o => o.Name == option).Value;
            return value is T ? (T)value : default(T);
        }
    }
}