using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TabletBot.Discord.Commands.Attributes;

namespace TabletBot.Discord.Commands
{
    [Module]
    public class UserCommands : CommandModule
    {
        [Command("tablet", RunMode = RunMode.Async), Name("Tablet"), Summary("Appends your tablet's name to the end of your username.")]
        public async Task SetTablet([Remainder]string tablet)
        {
            await Context.Message.DeleteAsync();
            var nickname = $"{Context.User.Username} | {tablet}";
            if (Context.User is IGuildUser guildUser)
                await guildUser.ModifyAsync(user => user.Nickname = nickname);
            else
                await ReplyAsync("Failed to set nickname: User is not in a guild.");
        }
    }
}