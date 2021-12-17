using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TabletBot.Common;

namespace TabletBot.Discord.Commands
{
    public class ModerationCommands : ModuleBase
    {
        private readonly Bot _bot;

        public ModerationCommands(Bot bot)
        {
            _bot = bot;
        }

        [Command("delete", RunMode = RunMode.Async), Name("Delete"), Alias("del"), RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task DeleteMessage(int count = 1)
        {
            await Context.Message.DeleteAsync();
            var messages = await Context.Channel.GetMessagesAsync(count).FlattenAsync();
            await (Context.Channel as ITextChannel).DeleteMessagesAsync(messages);
        }

        [Command("save", RunMode = RunMode.Async), Name("Force Save"), RequireOwner]
        public async Task ForceSaveSettings()
        {
            await Context.Message.DeleteAsync();
            await Settings.Current.Write(Platform.SettingsFile);
            await Log.WriteAsync("Settings", $"{Context.Message.Author.Username} force-saved the configuration to {Platform.SettingsFile.FullName}");
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
            await Context.Message.DeleteAsync();
            Settings.Current.CommandPrefix = prefix;
            var message = await ReplyAsync(string.Format("Set the command prefix to `{0}`.", Settings.Current.CommandPrefix));
            message.DeleteDelayed();
        }

        [Command("set-reply-delete-delay", RunMode = RunMode.Async), Name("Set bot reply delete delay"), RequireOwner]
        public async Task SetReplyDeleteDelay(TimeSpan delay)
        {
            await Context.Message.DeleteAsync();
            Settings.Current.DeleteDelay = (int)delay.TotalMilliseconds;
            
            var embed = new EmbedBuilder
            {
                Title = "Set message delay",
                Fields =
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Delay",
                        Value = Settings.Current.DeleteDelay
                    }
                }
            };
            var message = await ReplyAsync(embed: embed.Build());
            message.DeleteDelayed();
        }
    }
}