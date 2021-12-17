using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TabletBot.Common;
using TabletBot.Discord.SlashCommands;

namespace TabletBot.Discord
{
    public partial class Bot
    {
        private bool _slashCommandsRegistered;

        private IList<SlashCommandModule> SlashCommandModules { get; } = new List<SlashCommandModule>();

        public async Task UpdateSlashCommands()
        {
            foreach (var module in SlashCommandModules)
            {
                try
                {
                    await module.Update(DiscordClient);
                    await Log.WriteAsync("CommandSvc", $"Re-registered slash command module '{module.GetType().Name}'.", LogLevel.Debug);
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }
        }

        private async Task RegisterSlashCommands(IServiceProvider serviceProvider)
        {
            if (_slashCommandsRegistered)
                return;

            await Log.WriteAsync("Setup", "Registering slash commands...", LogLevel.Debug);
            foreach (var module in serviceProvider.GetServices<SlashCommandModule>())
            {
                try
                {
                    await module.Hook(DiscordClient);
                    SlashCommandModules.Add(module);
                    await Log.WriteAsync("Setup", $"Registered slash command module '{module.GetType().Name}'.",
                        LogLevel.Debug);
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }

            _slashCommandsRegistered = true;
            await Log.WriteAsync("Setup", "Successfully registered slash commands.", LogLevel.Debug);
        }
    }
}