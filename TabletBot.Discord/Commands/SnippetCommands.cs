using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TabletBot.Common;
using TabletBot.Common.Store;

namespace TabletBot.Discord.Commands
{
    public class SnippetCommands : CommandModule
    {
        private const string SHOW_SNIPPET = "snippet";
        private const string LIST_SNIPPETS = "list-snippets";
        private const string SET_SNIPPET = "set-snippet";
        private const string REMOVE_SNIPPET = "remove-snippet";
        private const string EXPORT_SNIPPET = "export-snippet";

        private static IList<SnippetStore> Snippets => Settings.Current.Snippets;

        [Command(SHOW_SNIPPET, RunMode = RunMode.Async), Name("Show snippet")]
        public async Task ShowSnippet(string prefix)
        {
            await Context.Message?.DeleteAsync();

            if (TryGetSnippetEmbed(prefix, out var embed))
            {
                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                var message = await ReplyAsync(embed: embed.Build(), messageReference: Context.Message.Reference);
                message.DeleteDelayed();
            }
        }

        [Command(LIST_SNIPPETS, RunMode = RunMode.Async), Name("List snippets")]
        public async Task ListSnippets()
        {
            await Context.Message?.DeleteAsync();

            if (Snippets.Any())
            {
                var embed = new EmbedBuilder
                {
                    Title = "Snippets"
                };

                foreach (var snippet in Snippets)
                {
                    embed.AddField(
                        CODE_AFFIX + snippet.Prefix + CODE_AFFIX,
                        "   " + snippet.Title
                    );
                }

                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                var embed = new EmbedBuilder
                {
                    Color = Color.Magenta,
                    Title = "Snippets",
                    Description = "No snippets have been created."
                };

                var message = await ReplyAsync(embed: embed.Build());
                message.DeleteDelayed();
            }
        }

        [Command(SET_SNIPPET, RunMode = RunMode.Async), Name("Create snippet"), RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetSnippet(string prefix, string title, [Remainder] string content)
        {
            await Context.Message?.DeleteAsync();

            if (Snippets.FirstOrDefault(s => s.Prefix == prefix) is SnippetStore snippet)
            {
                // Update the existing snippet
                snippet.Title = title;
                snippet.Content = content;
            }
            else
            {
                // Create a new snippet
                snippet = new SnippetStore(prefix, title, content);
                Snippets.Add(snippet);
            }
            await OverwriteSettings();
            await ReplyAsync(embed: GetSnippetEmbed(snippet).Build());
        }

        [Command(REMOVE_SNIPPET, RunMode = RunMode.Async), Name("Delete snippet"), RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemoveSnippet(string prefix)
        {
            await Context.Message?.DeleteAsync();

            EmbedBuilder result;

            if (Snippets.FirstOrDefault(t => t.Prefix == prefix) is SnippetStore snippet)
            {
                Snippets.Remove(snippet);
                result = new EmbedBuilder
                {
                    Color = Color.Green,
                    Title = "Successfully removed snippet",
                    Fields =
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Prefix",
                            Value = prefix
                        }
                    }
                };
                await OverwriteSettings();
            }
            else
            {
                result = new EmbedBuilder
                {
                    Color = Color.Red,
                    Title = "Failed to delete snipppet",
                    Description = "The snippet was not found."
                };
            }

            var message = await ReplyAsync(embed: result.Build());
            message.DeleteDelayed();
        }

        [Command(EXPORT_SNIPPET, RunMode = RunMode.Async), Name("Export snippet")]
        public async Task ExportSnippet(string prefix)
        {
            await Context.Message?.DeleteAsync();

            if (Snippets.FirstOrDefault(s => s.Prefix == prefix) is SnippetStore snippet)
            {
                var sb = new StringBuilder();
                sb.AppendLine(CODE_BLOCK);
                sb.AppendLine(snippet.Content);
                sb.AppendLine(CODE_BLOCK);

                await ReplyAsync(sb.ToString());
            }
            else
            {
                var result = new EmbedBuilder
                {
                    Color = Color.Red,
                    Title = "Failed to export snippet",
                    Description = "The snippet was not found."
                };

                var message = await ReplyAsync(embed : result.Build());
                message.DeleteDelayed();
            }
        }

        private bool TryGetSnippetEmbed(string prefix, out EmbedBuilder embed)
        {
            if (Snippets.FirstOrDefault(s => s.Prefix == prefix) is SnippetStore snippet)
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

        private EmbedBuilder GetSnippetEmbed(SnippetStore snippet)
        {
            return new EmbedBuilder
            {
                Title = snippet.Title,
                Color = Color.Magenta,
                Description = snippet.Content,
                Timestamp = Context.Message.Timestamp,
                Footer = Context.User.ToEmbedFooter($"`{snippet.Prefix}` {{0}}")
            };
        }
    }
}