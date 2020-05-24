using Discord;
using Octokit;

namespace TabletBot.Discord.Embeds
{
    public static class GitHubEmbeds
    {
        public static EmbedBuilder GetEmbed(Issue issue)
        {
            return new EmbedBuilder
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
        }

        public static EmbedBuilder GetEmbed(PullRequest pr)
        {
            return new EmbedBuilder
            {
                Title = string.Format("{0} #{1}", pr.Title, pr.Number),
                Timestamp = pr.UpdatedAt,
                Url = pr.HtmlUrl,
                Footer = new EmbedFooterBuilder
                {
                    Text = string.Format("{0} opened this pull request on {1}", pr.User.Login, pr.CreatedAt),
                    IconUrl = pr.User.AvatarUrl
                },
                Description = pr.Body
            };
        }
    }
}