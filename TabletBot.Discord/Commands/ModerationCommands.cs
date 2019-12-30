using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TabletBot.Common;

namespace TabletBot.Discord.Commands
{
    public class ModerationCommands : ModuleBase
    {
        [Command("delete", RunMode = RunMode.Async), Name("Delete"), Alias("del"), RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task DeleteMessage(int count = 1)
        {
            await Context.Message.DeleteAsync();
            var messages = await Context.Channel.GetMessagesAsync(count).FlattenAsync();
            await (Context.Channel as ITextChannel).DeleteMessagesAsync(messages);
        }

        [Command("force-save", RunMode = RunMode.Async), Name("Force Save"), RequireOwner]
        public async Task ForceSaveSettings()
        {
            await Context.Message.DeleteAsync();
            Settings.Current.Write(Platform.SettingsFile);
            await Log.WriteAsync("Settings", $"Owner force-saved the configuration to {Platform.SettingsFile.FullName}");
        }

        [Command("kill-bot", RunMode = RunMode.Async), Name("Kill Bot"), RequireOwner]
        public async Task ForceKillBot()
        {
            await Context.Message.DeleteAsync();
            await Bot.Current.Logout();
            Environment.Exit(0x0);
        }

        [Command("set-prefix", RunMode = RunMode.Async), Name("Set prefix"), RequireOwner]
        public async Task SetPrefix([Remainder] string prefix)
        {
            await Context.Message.DeleteAsync();
            Settings.Current.CommandPrefix = prefix;
            var message = await ReplyAsync(string.Format("Set the command prefix to `{0}`.", Settings.Current.CommandPrefix));
            await message.DeleteDelayed();
        }
    }
}