using System;
using System.Collections.Generic;
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
        private const string CREATE_SNIPPET = "create-snippet";
        private const string DELETE_SNIPPET = "delete-snippet";

        private static IList<SnippetStore> Snippets => Settings.Current.Snippets;

        [Command(SHOW_SNIPPET, RunMode = RunMode.Async), Name("Show snippet")]
        public async Task ShowSnippet(string prefix)
        {
            await Context.Message?.DeleteAsync();

            if (Snippets.FirstOrDefault(s => s.Prefix == prefix) is SnippetStore snippet)
            {
                var embed = new EmbedBuilder
                {
                    Color = Color.Magenta,
                    Description = snippet.Content,
                    Timestamp = Context.Message.Timestamp,
                    Footer = Context.User.ToEmbedFooter()
                };
                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                var embed = new EmbedBuilder
                {
                    Title = "Failed to show snippet",
                    Description = $"Failed to show the `{prefix}` snippet." + Environment.NewLine
                        + "Verify that you have spelled it correctly."
                };
                var message = await ReplyAsync(embed: embed.Build());
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
                    Title = "Snippets",
                    Description = string.Join(Environment.NewLine, Snippets.Select(s => s.Prefix))
                };

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

        [Command(CREATE_SNIPPET, RunMode = RunMode.Async), Name("Create snippet"), RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task CreateSnippet(string prefix, [Remainder] string content)
        {
            await Context.Message?.DeleteAsync();

            EmbedBuilder result;

            if (!Snippets.Any(s => s.Prefix == prefix))
            {
                var snippet = new SnippetStore(prefix, content);
                Snippets.Add(snippet);

                result = new EmbedBuilder
                {
                    Color = Color.Green,
                    Title = "Successfully created snippet",
                    Fields =
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Prefix",
                            Value = prefix
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Content",
                            Value = content
                        }
                    }
                };
            }
            else
            {
                result = new EmbedBuilder
                {
                    Color = Color.Red,
                    Title = "Failed to create snippet",
                    Description = $"The snippet for the prefix `{prefix}` has already been created." + Environment.NewLine
                        + $"You can delete the snippet with the `{DELETE_SNIPPET}` command."
                };
            }

            var message = await ReplyAsync(embed: result.Build());
            message.DeleteDelayed();
        }

        [Command(DELETE_SNIPPET, RunMode = RunMode.Async), Name("Delete snippet"), RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task DeleteSnippet(string prefix)
        {
            await Context.Message?.DeleteAsync();

            EmbedBuilder result;

            if (Snippets.FirstOrDefault(t => t.Prefix == prefix) is SnippetStore snippet)
            {
                Snippets.Remove(snippet);
                result = new EmbedBuilder
                {
                    Color = Color.Green,
                    Title = "Successfully deleted snippet",
                    Fields =
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Prefix",
                            Value = prefix
                        }
                    }
                };
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
    }
}