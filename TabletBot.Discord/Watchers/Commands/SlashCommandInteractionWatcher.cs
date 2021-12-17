using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using TabletBot.Common;
using TabletBot.Discord.SlashCommands;

namespace TabletBot.Discord.Watchers.Commands
{
    public class SlashCommandInteractionWatcher : IInteractionWatcher, IAsyncInitialize
    {
        public SlashCommandInteractionWatcher(
            DiscordSocketClient discordClient,
            IEnumerable<SlashCommandModule> commands
        )
        {
            _client = discordClient;
            _commands = commands;
        }

        private readonly DiscordSocketClient _client;
        private readonly IEnumerable<SlashCommandModule> _commands;

        private bool _registered;

        public async Task HandleInteraction(SocketInteraction interaction)
        {
            await Task.WhenAll(_commands.Select(m => m.HandleInteraction(interaction)));
        }

        public async Task InitializeAsync()
        {
            if (_registered)
                return;

            await Task.WhenAll(_commands.Select(RegisterCommand));
            _registered = true;
        }

        private async Task RegisterCommand(SlashCommandModule module)
        {
            module.BuildCommandHandlers();

            var moderatorCommands = new List<RestGuildCommand>();
            foreach (var command in module.CommandHandlers)
            {
                var guildCommand = await _client.Rest.CreateGuildCommand(command.Build(), Settings.Current.GuildID);
                if (guildCommand.IsDefaultPermission == false)
                    moderatorCommands.Add(guildCommand);
            }

            await ApplyCommandPermissions(_client, moderatorCommands);

            module.Update += UpdateModule;
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
                    var perms = new[]
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

        private async Task UpdateModule(SlashCommandModule module)
        {
            module.BuildCommandHandlers();

            foreach (var command in await _client.Rest.GetGuildApplicationCommands(Settings.Current.GuildID))
                await command.DeleteAsync();

            var moderatorCommands = new List<RestGuildCommand>();
            foreach (var command in module.CommandHandlers)
            {
                var guildCommand = await _client.Rest.CreateGuildCommand(command.Build(), Settings.Current.GuildID);
                if (guildCommand.IsDefaultPermission == false)
                    moderatorCommands.Add(guildCommand);
            }

            await ApplyCommandPermissions(_client, moderatorCommands);
        }
    }
}