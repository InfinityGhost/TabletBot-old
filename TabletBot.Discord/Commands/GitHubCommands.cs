using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Octokit;
using TabletBot.GitHub;

namespace TabletBot.Discord.Commands
{
    public class GitHubCommands : CommandModule
    {
        private GitHubAPI GitHub => TabletBot.GitHub.GitHubAPI.Current;
        private const string RepositoryOwner = "InfinityGhost";
        private const string RepositoryName = "OpenTabletDriver";

        [Command("overview"), Alias("info")]
        public async Task GetRepositoryOverview()
        {
            var message = await ReplyAsync($"Getting overview for {RepositoryOwner}/{RepositoryName}...");
            var repo = await GitHub.Repository.Get(RepositoryOwner, RepositoryName);

            IEnumerable<Issue> issues =
                from issue in await GitHub.Issue.GetAllForRepository(repo.Id)
                where issue.State == ItemState.Open
                select issue;

            IEnumerable<PullRequest> prs =
                from pr in await GitHub.PullRequest.GetAllForRepository(repo.Id)
                where pr.State == ItemState.Open
                select pr;

            var embed = new EmbedBuilder
            {
                Title = $"{RepositoryOwner}/{RepositoryName}",
                Timestamp = repo.PushedAt,
                ThumbnailUrl = Bot.Current.DiscordClient.CurrentUser.GetAvatarUrl(),
                Footer = new EmbedFooterBuilder
                {
                    IconUrl = repo.Owner.AvatarUrl
                }
            };
            embed.AddField("Open issues", issues.Count());
            embed.AddField("Open pull requests", prs.Count());
            await message.Update(embed);
        }

        [Command("getpr"), Alias("pr")]
        public async Task GetPullRequest([Remainder] int id)
        {
            var message = await ReplyAsync($"Fetching pull request #{id}");
            var pr = await GitHub.PullRequest.Get(RepositoryOwner, RepositoryName, id);
            var embed = new EmbedBuilder
            {
                Title = string.Format("{0} #{1}", pr.Title, pr.Id),
                Timestamp = pr.UpdatedAt,
                Url = pr.HtmlUrl,
                Footer = new EmbedFooterBuilder
                {
                    Text = string.Format("{0} opened this pull request on {1}", pr.User.Login, pr.CreatedAt),
                    IconUrl = pr.User.AvatarUrl
                }
            };
            embed.AddField("Body", pr.Body);
            await message.Update(embed);
        }

        [Command("getissue"), Alias("issue")]
        public async Task GetIssue([Remainder] int id)
        {
            var message = await ReplyAsync($"Fetching issue #{id}");
            var issue = await GitHub.Issue.Get(RepositoryOwner, RepositoryName, id);
            var embed = new EmbedBuilder
            {
                Title = string.Format("{0} #{1}", issue.Title, issue.Id),
                Timestamp = issue.CreatedAt,
                Url = issue.HtmlUrl,
                Footer = new EmbedFooterBuilder
                {
                    Text = string.Format("{0} opened this issue on {1}", issue.User.Login, issue.CreatedAt),
                    IconUrl = issue.User.AvatarUrl
                }
            };
            embed.AddField("Body", issue.Body);
            await message.Update(embed);
        }
    }
}