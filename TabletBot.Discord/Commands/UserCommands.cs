using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace TabletBot.Discord.Commands
{
    public class UserCommands : CommandModule
    {
        [Command("tablet", RunMode = RunMode.Async), Name("Tablet"), Summary("Appends your tablet's name to the end of your username.")]
        public async Task SetTablet([Remainder]string tablet)
        {
            await Context.Message.DeleteAsync();
            var nickname = string.Format("{0} | {1}", Context.User.Username, tablet);
            await (Context.User as IGuildUser).ModifyAsync(user => user.Nickname = nickname);
        }
    }
}