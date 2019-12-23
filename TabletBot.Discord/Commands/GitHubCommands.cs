using System.Threading.Tasks;
using Discord.Commands;

namespace TabletBot.Discord.Commands
{
    public class GitHubCommands : ModuleBase
    {
        public GitHubCommands()
        {
        }

        [Command("getpr"), Alias("pr")]
        public async Task GetPullRequest([Remainder] int id)
        {
            
        }
    }
}