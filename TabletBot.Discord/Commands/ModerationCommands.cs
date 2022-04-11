using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TabletBot.Common;
using TabletBot.Discord.Commands.Attributes;

namespace TabletBot.Discord.Commands
{
    [Module]
    public class ModerationCommands : CommandModule
    {
        private readonly Bot _bot;
        private readonly Settings _settings;

        public ModerationCommands(Bot bot, Settings settings)
        {
            _bot = bot;
            _settings = settings;
        }

        [Command("delete", RunMode = RunMode.Async), Name("Delete"), Alias("del"), RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task DeleteMessage(int count = 1)
        {
            await Context.Message.DeleteAsync();
            var messages = await Context.Channel.GetMessagesAsync(count).FlattenAsync();
            if (Context.Channel is ITextChannel textChannel)
                await textChannel.DeleteMessagesAsync(messages);
            else
                await ReplyAsync("Unable to delete messages as the current channel is not a text channel.");
        }

        [Command("save", RunMode = RunMode.Async), Name("Force Save"), RequireOwner]
        public async Task ForceSaveSettings()
        {
            await _settings.Write(Platform.SettingsFile);
            await ReplyAsync("Settings force saved.", allowedMentions: AllowedMentions.None);
            Log.Write("Settings", $"{Context.Message.Author.Username} force-saved the configuration to {Platform.SettingsFile.FullName}");
        }

        [Command("kill-bot", RunMode = RunMode.Async), Name("Kill Bot"), RequireOwner]
        public async Task ForceKillBot()
        {
            await Context.Message.DeleteAsync();
            await _bot.Logout();
            Environment.Exit(0x0);
        }

        [Command("set-prefix", RunMode = RunMode.Async), Name("Set prefix"), RequireOwner]
        public async Task SetPrefix([Remainder] string prefix)
        {
            _settings.CommandPrefix = prefix;
            await ReplyAsync($"Set the command prefix to `{_settings.CommandPrefix}`.");
        }
    }
}