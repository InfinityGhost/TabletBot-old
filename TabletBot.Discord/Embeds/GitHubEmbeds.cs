using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Octokit;

namespace TabletBot.Discord.Embeds
{
    public static class GitHubEmbeds
    {
        private const uint OPEN_COLOR = 0x238636;
        private const uint RESOLVED_COLOR = 0x8957e5;
        private const uint CLOSED_COLOR = 0xda3633;

        public static EmbedBuilder GetEmbed(Issue? issue)
        {
            if (issue == null)
            {
                return new EmbedBuilder
                {
                    Color = CLOSED_COLOR,
                    Description = "No issue or pull request was found."
                };
            }

            return new EmbedBuilder
            {
                Title = $"{issue.Title} #{issue.Number}",
                Url = issue.HtmlUrl,
                Description = LimitLength(issue.Body, 750),
                Color = GetColor(issue),
                Fields = GetFields(issue).ToList(),
                Author = new EmbedAuthorBuilder
                {
                    Name = issue.User.Login,
                    Url = issue.User.HtmlUrl,
                    IconUrl = issue.User.AvatarUrl
                }
            };
        }

        private static uint GetColor(Issue issue)
        {
            if (issue.ClosedAt == null)
                return OPEN_COLOR;

            if (issue.PullRequest != null)
                return issue.PullRequest.Merged ? RESOLVED_COLOR : CLOSED_COLOR;

            return RESOLVED_COLOR;
        }

        private static IEnumerable<EmbedFieldBuilder> GetFields(Issue issue)
        {
            if (issue.Milestone != null)
            {
                yield return new EmbedFieldBuilder
                {
                    Name = "Milestone",
                    Value = Formatting.UrlString(issue.Milestone.Title, issue.Milestone.HtmlUrl),
                    IsInline = true
                };
            }

            if (issue.Labels.Any())
            {
                yield return new EmbedFieldBuilder
                {
                    Name = "Labels",
                    Value = string.Join(", ", issue.Labels.Select(l => Formatting.CodeString(l.Name))),
                    IsInline = true
                };
            }
        }

        private static string LimitLength(string? str, int characterLimit)
        {
            if (string.IsNullOrWhiteSpace(str))
                return string.Empty;

            return str.Length > characterLimit ? str[..(characterLimit-3)] + "..." : str;
        }
    }
}