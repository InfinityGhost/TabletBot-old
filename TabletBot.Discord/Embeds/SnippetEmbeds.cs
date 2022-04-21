using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using TabletBot.Common.Store;

namespace TabletBot.Discord.Embeds
{
    public static class SnippetEmbeds
    {
        public static bool GetSnippetEmbed(this IEnumerable<Snippet> snippets, string? prefix, out EmbedBuilder embed)
        {
            if (snippets.FirstOrDefault(s => s.ID == prefix!) is Snippet snippet)
            {
                embed = new EmbedBuilder
                {
                    Title = snippet.Title,
                    Color = Color.Magenta,
                    Description = snippet.Content
                };
                return true;
            }

            embed = new EmbedBuilder
            {
                Color = Color.Red,
                Title = "Failed to show snippet",
                Description = $"Failed to show the `{prefix}` snippet." + Environment.NewLine + "Verify that you have spelled it correctly."
            };
            return false;
        }
    }
}