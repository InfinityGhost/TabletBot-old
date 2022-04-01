using Discord;
using Octokit;

namespace TabletBot.Discord.Embeds
{
    public static class GitHubEmbeds
    {
        public static Embed GetIssueEmbed(Issue issue)
        {
            var embed = new EmbedBuilder
            {
                Title = string.Format("{0} #{1}", issue.Title, issue.Number),
                Timestamp = issue.CreatedAt,
                Url = issue.HtmlUrl,
                Footer = new EmbedFooterBuilder
                {
                    Text = string.Format("{0} opened this issue on {1}", issue.User.Login, issue.CreatedAt),
                    IconUrl = issue.User.AvatarUrl
                },
                Description = issue.Body
            };
            return embed.Build();
        }

        public static Embed GetPullRequestEmbed(PullRequest pr)
        {
            var embed = new EmbedBuilder
            {
                Title = $"{pr.Title} #{pr.Number}",
                Timestamp = pr.UpdatedAt,
                Url = pr.HtmlUrl,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"{pr.User?.Login} opened this pull request on {pr.CreatedAt}",
                    IconUrl = pr.User?.AvatarUrl
                },
                Description = pr.Body ?? string.Empty
            };
            return embed.Build();
        }
    }
}