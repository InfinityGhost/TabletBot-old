using System;
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
        private readonly Settings _settings;
        private readonly DiscordSocketClient _client;
        private readonly IEnumerable<SlashCommandModule> _commands;

        public SlashCommandInteractionWatcher(
            Settings settings,
            DiscordSocketClient client,
            IEnumerable<SlashCommandModule> commands
        )
        {
            _settings = settings;
            _client = client;
            _commands = commands;
        }

        private bool _registered;

        public async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                await Task.WhenAll(_commands.Select(m => m.HandleInteraction(interaction)));
            }
            catch (Exception e)
            {
                Log.Exception(e);
                try
                {
                    string message = e.GetType().FullName + ": " + e.Message;
                    if (interaction.HasResponded)
                    {
                        await interaction.FollowupAsync(message, ephemeral: true);
                    }
                    else
                    {
                        await interaction.RespondAsync(message, ephemeral: true);
                    }
                }
                catch (Exception responseEx)
                {
                    Log.Exception(responseEx);
                }

                throw;
            }
        }

        public async Task InitializeAsync()
        {
            if (_registered)
                return;

            await Task.WhenAll(_commands.Select(RegisterCommandModule));
            _registered = true;
        }

        private async Task RegisterCommandModule(SlashCommandModule module)
        {
            var moderatorCommands = new List<RestGuildCommand>();
            foreach (var command in module.BuildCommandHandlers())
            {
                var guildCommand = await _client.Rest.CreateGuildCommand(command.Build(), _settings.GuildID);
                if (guildCommand.IsDefaultPermission == false)
                    moderatorCommands.Add(guildCommand);
            }

            await ApplyCommandPermissions(_client, moderatorCommands);
            module.Update += UpdateModule;

            Log.Write("Setup", $"Registered slash command module '{module.GetType().Name}'.");
        }

        private async Task ApplyCommandPermissions(DiscordSocketClient client, IEnumerable<RestGuildCommand> moderatorCommands)
        {
            var guild = client.GetGuild(_settings.GuildID);
            var modRole = guild.GetRole(_settings.ModeratorRoleID);

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
                    await client.Rest.BatchEditGuildCommandPermissions(_settings.GuildID, permDict);
                }
            }
        }

        private async Task UpdateModule(SlashCommandModule module)
        {
            var applicationCommands = await _client.Rest.GetGuildApplicationCommands(_settings.GuildID);
            var handlers = module.BuildCommandHandlers();

            var commandsToUpdate = from handler in handlers
                let command = applicationCommands.FirstOrDefault(c => c.Name == handler.Name)
                where command != null
                select (handler, command);

            Log.Write("SlashCmd", $"Updating slash commands...");
            var moderatorCommands = new List<RestGuildCommand>();
            foreach (var updateable in commandsToUpdate)
            {
                await updateable.command.DeleteAsync();

                var guildCommand = await _client.Rest.CreateGuildCommand(updateable.handler.Build(), _settings.GuildID);
                if (guildCommand.IsDefaultPermission == false)
                    moderatorCommands.Add(guildCommand);

                Log.Write("SlashCmd", $"Successfully updated slash command {updateable.handler.Name}.");
            }

            await ApplyCommandPermissions(_client, moderatorCommands);
        }
    }
}