using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using TabletBot.Common;

namespace TabletBot.Discord.SlashCommands
{
    public abstract class SlashCommandModule
    {
        public virtual async Task Hook(DiscordSocketClient client)
        {
            BuildCommandHandlers();

            var moderatorCommands = new List<RestGuildCommand>();
            foreach (var command in CommandHandlers)
            {
                var guildCommand = await client.Rest.CreateGuildCommand(command.Build(), Settings.Current.GuildID);
                if (guildCommand.DefaultPermission == false)
                    moderatorCommands.Add(guildCommand);
            }

            await ApplyCommandPermissions(client, moderatorCommands);

            client.InteractionCreated += HandleInteraction;
        }

        public virtual async Task Update(DiscordSocketClient client)
        {
            BuildCommandHandlers();

            foreach (var command in await client.Rest.GetGuildApplicationCommands(Settings.Current.GuildID))
                await command.DeleteAsync();

            var moderatorCommands = new List<RestGuildCommand>();
            foreach (var command in CommandHandlers)
            {
                var guildCommand = await client.Rest.CreateGuildCommand(command.Build(), Settings.Current.GuildID);
                if (guildCommand.DefaultPermission == false)
                    moderatorCommands.Add(guildCommand);
            }

            await ApplyCommandPermissions(client, moderatorCommands);
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

        private async Task ApplyCommandPermissions(DiscordSocketClient client, IEnumerable<RestGuildCommand> moderatorCommands)
        {
            var guild = client.GetGuild(Settings.Current.GuildID);
            var modRole = guild.GetRole(Settings.Current.ModeratorRoleID);

            if (modRole != null)
            {
                var permDict = new Dictionary<ulong, ApplicationCommandPermission[]>();
                foreach (var command in moderatorCommands)
                {
                    var perms = new ApplicationCommandPermission[]
                    {
                        new ApplicationCommandPermission(modRole, true)
                    };
                    permDict.Add(modRole.Id, perms);
                }

                if (permDict.Any())
                {
                    await client.Rest.BatchEditGuildCommandPermissions(Settings.Current.GuildID, permDict);
                }
            }
        }
    }
}