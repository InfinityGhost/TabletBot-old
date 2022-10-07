using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Octokit;
using TabletBot.Common;
using TabletBot.Discord.Embeds;
using TabletBot.Discord.Commands;
using TabletBot.Discord.Watchers.Safe;

namespace TabletBot.Discord.Watchers.GitHub
{
    public class IssueMessageWatcher : SafeMessageWatcher
    {
        private readonly Settings _settings;
        private readonly GitHubClient _gitHubClient;

        public IssueMessageWatcher(Settings settings, GitHubClient gitHubClient)
        {
            _settings = settings;
            _gitHubClient = gitHubClient;
        }

        private const string OWNER = "OpenTabletDriver";
        private const string NAME = "OpenTabletDriver";

        private static readonly Regex IssueRefRegex = new Regex(@" ?#([0-9]+[0-9]) ?");

        protected override async Task ReceiveInternal(IMessage message)
        {
            if (message.Author.IsBot || message is not IUserMessage userMessage)
                return;

            var rateLimits = await _gitHubClient.Miscellaneous.GetRateLimits();
            if (rateLimits.Rate.Remaining < 2)
                return;

            if (GetIssueRefNumbers(userMessage.Content) is IList<uint> refs)
            {
                using (userMessage.Channel.EnterTypingState())
                {
                    await ReplyWithEmbeds(userMessage, refs);
                }
            }
        }

        private async Task ReplyWithEmbeds(IUserMessage message, IList<uint> refs)
        {
            var embeds = new Embed[refs.Count];

            for (var i = 0; i < refs.Count && i < _settings.GitHubIssueRefLimit; i++)
            {
                var issueRef = (int)refs[i];
                var issue = await _gitHubClient.Issue.Get(OWNER, NAME, issueRef);

                embeds[i] = GitHubEmbeds.GetEmbed(issue).Build();
            }

            await message.Channel.SendMessageAsync(embeds: embeds.ToArray(), messageReference: message.ToReference());
        }

        private static IEnumerable<uint>? GetIssueRefNumbers(string message)
        {
            var matches = IssueRefRegex.Matches(message);
            return matches.Any() ? matches.Select(m => uint.Parse(m.Groups[1].Value)).ToList() : null;
        }
    }
}