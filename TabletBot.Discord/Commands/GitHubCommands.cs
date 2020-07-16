using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Octokit;
using TabletBot.Common;
using TabletBot.Discord.Embeds;
using TabletBot.GitHub;

namespace TabletBot.Discord.Commands
{
    public class GitHubCommands : CommandModule
    {
        private GitHubAPI GitHub => TabletBot.GitHub.GitHubAPI.Current;
        private const string RepositoryOwner = "InfinityGhost";
        private const string RepositoryName = "OpenTabletDriver";

        [Command("overview", RunMode = RunMode.Async), Name("Overview"), Alias("info"), Summary("Shows an overview of the repository.")]
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
                Url = repo.HtmlUrl,
                ThumbnailUrl = Bot.Current.DiscordClient.CurrentUser.GetAvatarUrl(),
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Last pushed {repo.PushedAt}",
                    IconUrl = repo.Owner.AvatarUrl
                }
            };
            embed.AddField("Open issues", issues.Count());
            embed.AddField("Open pull requests", prs.Count());
            await message.Update(embed);
        }

        [Command("getpr", RunMode = RunMode.Async), Name("Get Pull Request"), Alias("pr"), Summary("Fetches pull request information.")]
        public async Task GetPullRequest([Remainder] int id)
        {
            await Context.Message.DeleteAsync();
            var message = await ReplyAsync($"Fetching pull request #{id}");
            var pr = await GitHub.PullRequest.Get(RepositoryOwner, RepositoryName, id);
            var embed = GitHubEmbeds.GetPullRequestEmbed(pr);
            await message.Update(embed);
        }

        [Command("getissue", RunMode = RunMode.Async), Name("Get Issue"), Alias("issue"), Summary("Fetches issue information.")]
        public async Task GetIssue([Remainder] int id)
        {
            await Context.Message.DeleteAsync();
            var message = await ReplyAsync($"Fetching issue #{id}");
            var issue = await GitHub.Issue.Get(RepositoryOwner, RepositoryName, id);
            var embed = GitHubEmbeds.GetIssueEmbed(issue);
            await message.Update(embed);
        }

        [Command("getartifacts", RunMode = RunMode.Async), Name("Get Artifacts"), Alias("artifacts", "artifact"), Summary("Returns all artifacts for a workflow from its URL.")]
        public async Task GetArtifacts([Remainder] string url)
        {
            await Context.Message.DeleteAsync();
            var message = await ReplyAsync("Fetching artifacts...");

            var artifacts = await Artifact.GetArtifactsForRun(RepositoryOwner, RepositoryName, url);
            
            var embed = new EmbedBuilder
            {
                Title = "Artifacts",
                Url = url,
                Color = Color.Green
            };

            foreach (var artifact in artifacts)
            {
                var token = GitHub.Credentials.GetToken();
                var download = await artifact.GetDownloadRedirect(token);
                embed.AddField(artifact.Name, download);
            }

            await message.Update(embed);
        }
    }
}