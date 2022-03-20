using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Octokit;
using TabletBot.Discord.Commands.Attributes;
using TabletBot.Discord.Embeds;

namespace TabletBot.Discord.Commands
{
    [Module]
    public class GitHubCommands : CommandModule
    {
        private readonly GitHubClient _gitHubClient;
        private readonly DiscordSocketClient _discordSocketClient;

        public GitHubCommands(GitHubClient gitHubClient, DiscordSocketClient discordSocketClient)
        {
            _gitHubClient = gitHubClient;
            _discordSocketClient = discordSocketClient;
        }

        private const string REPOSITORY_OWNER = "InfinityGhost";
        private const string REPOSITORY_NAME = "OpenTabletDriver";
        private static readonly Regex ArtifactRegex = new Regex("<.+?href=\"/InfinityGhost/OpenTabletDriver/suites/(?<Suite>.+?)/artifacts/(?<Artifact>.+?)\">(?<Name>.+?)</.+?>");
        private static readonly Regex CommitRegex = new Regex("href=\".+?commit/(?<SHA>.+?)/.+?/.+?\"");

        [Command("overview", RunMode = RunMode.Async), Name("Overview"), Alias("info"), Summary("Shows an overview of the repository.")]
        public async Task GetRepositoryOverview()
        {
            var message = await ReplyAsync($"Getting overview for {REPOSITORY_OWNER}/{REPOSITORY_NAME}...");
            var repo = await _gitHubClient.Repository.Get(REPOSITORY_OWNER, REPOSITORY_NAME);

            IEnumerable<Issue> issues =
                from issue in await _gitHubClient.Issue.GetAllForRepository(repo.Id)
                where issue.State == ItemState.Open
                select issue;

            IEnumerable<PullRequest> prs =
                from pr in await _gitHubClient.PullRequest.GetAllForRepository(repo.Id)
                where pr.State == ItemState.Open
                select pr;

            var embed = new EmbedBuilder
            {
                Title = $"{REPOSITORY_OWNER}/{REPOSITORY_NAME}",
                Timestamp = repo.PushedAt,
                Url = repo.HtmlUrl,
                ThumbnailUrl = _discordSocketClient.CurrentUser.GetAvatarUrl(),
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
            var message = await ReplyAsync($"Fetching pull request #{id}");
            var pr = await _gitHubClient.PullRequest.Get(REPOSITORY_OWNER, REPOSITORY_NAME, id);
            var embed = GitHubEmbeds.GetPullRequestEmbed(pr);
            await message.Update(embed);
        }

        [Command("getissue", RunMode = RunMode.Async), Name("Get Issue"), Alias("issue"), Summary("Fetches issue information.")]
        public async Task GetIssue([Remainder] int id)
        {
            var message = await ReplyAsync($"Fetching issue #{id}");
            var issue = await _gitHubClient.Issue.Get(REPOSITORY_OWNER, REPOSITORY_NAME, id);
            var embed = GitHubEmbeds.GetIssueEmbed(issue);
            await message.Update(embed);
        }

        [Command("getartifacts", RunMode = RunMode.Async), Name("Get Artifacts"), Alias("artifacts", "artifact"), Summary("Returns all artifacts for a workflow from its URL.")]
        public async Task GetArtifacts([Remainder] string url)
        {
            var message = await ReplyAsync("Fetching artifacts...");

            string html;
            using (var client = new HttpClient())
                html = await client.GetStringAsync(url);

            IEnumerable<Match> artifacts = await Task.Run(() => ArtifactRegex.Matches(html));
            var commitMatch = await Task.Run(() => CommitRegex.Match(html));
            var sha = commitMatch.Groups["SHA"].Value;
            var hash = string.Concat(sha.Take(7));
            var commit = await _gitHubClient.Git.Commit.Get(REPOSITORY_OWNER, REPOSITORY_NAME, sha);
            var title = commit.Message.Split(Environment.NewLine).First();
            
            var embed = new EmbedBuilder
            {
                Title = $"{title} ({hash})",
                Url = url,
                Color = Color.Green
            };

            foreach (var artifact in artifacts)
            {
                var suite = Convert.ToInt32(artifact.Groups["Suite"].Value);
                var id = Convert.ToInt32(artifact.Groups["Artifact"].Value);
                var name = artifact.Groups["Name"].Value;
                embed.AddField(name, string.Format("https://github.com/InfinityGhost/OpenTabletDriver/suites/{0}/artifacts/{1}", suite, id));
            }

            await message.Update(embed);
        }
    }
}