using System;
using System.Linq;
using Discord;
using TabletBot.Common;
using TabletBot.Common.Store;

namespace TabletBot.Discord.Embeds
{
    public static class SnippetEmbeds
    {
        public static bool TryGetSnippetEmbed(Settings settings, string prefix, out EmbedBuilder embed)
        {
            if (settings.Snippets.FirstOrDefault(s => s.Snippet == prefix) is SnippetStore snippet)
            {
                embed = GetSnippetEmbed(snippet);
                return true;
            }
            else
            {
                embed = new EmbedBuilder
                {
                    Color = Color.Red,
                    Title = "Failed to show snippet",
                    Description = $"Failed to show the `{prefix}` snippet." + Environment.NewLine
                        + "Verify that you have spelled it correctly."
                };
                return false;
            }
        }

        public static EmbedBuilder GetSnippetEmbed(SnippetStore snippet)
        {
            return new EmbedBuilder
            {
                Title = snippet.Title,
                Color = Color.Magenta,
                Description = snippet.Content
            };
        }
    }
}