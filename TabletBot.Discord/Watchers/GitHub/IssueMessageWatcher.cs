using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Octokit;
using TabletBot.Common;
using TabletBot.Discord.Embeds;

#nullable enable

namespace TabletBot.Discord.Watchers.GitHub
{
    public class IssueMessageWatcher : IMessageWatcher
    {
        private GitHubClient _gitHubClient;

        public IssueMessageWatcher(GitHubClient gitHubClient)
        {
            _gitHubClient = gitHubClient;
        }

        private const string OWNER = "OpenTabletDriver";
        private const string NAME = "OpenTabletDriver";

        public async Task Receive(IMessage message)
        {
            if (message.Author.IsBot)
                return;

            if (TryGetIssueRefNumbers(message.Content, out var refs))
            {
                uint refNum = 0;
                foreach (int issueRef in refs)
                {
                    if (refNum == Settings.Current.GitHubIssueRefLimit)
                        break;

                    var issues = await _gitHubClient.Issue.GetAllForRepository(OWNER, NAME);
                    var prs = await _gitHubClient.PullRequest.GetAllForRepository(OWNER, NAME);
                    if (issues.FirstOrDefault(i => i.Number == issueRef) is Issue issue)
                    {
                        var pr = prs.FirstOrDefault(pr => pr.Number == issue.Number);
                        var embed = pr == null ? GitHubEmbeds.GetIssueEmbed(issue) : GitHubEmbeds.GetPullRequestEmbed(pr);
                        await message.Channel.SendMessageAsync(embed: embed);
                    }
                    refNum++;
                }
            }
        }

        public Task Deleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel) => Task.CompletedTask;

        public static bool TryGetIssueRefNumbers(string message, out IEnumerable<int> refNums)
        {
            refNums = Array.Empty<int>();

            var matches = IssueRefRegex.Matches(message);
            if (matches.Count > 0)
            {
                refNums = from match in matches
                    select int.Parse(match.Groups[1].Value);
                return true;
            }

            return false;
        }

        private static readonly Regex IssueRefRegex = new Regex(@" ?#([0-9]+[0-9]) ?");
    }
}