using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TabletBot.Common;
using TabletBot.Common.Store;
using TabletBot.Discord.Commands.Attributes;
using TabletBot.Discord.Embeds;
using TabletBot.Discord.SlashCommands;

namespace TabletBot.Discord.Commands
{
    [Module]
    public class SnippetCommands : CommandModule
    {
        public SnippetCommands(State state, IEnumerable<SlashCommandModule> slashCommands)
        {
            _state = state;
            _snippetSlashCommands = slashCommands.FirstOfType<SlashCommandModule, SnippetSlashCommands>();
        }

        private readonly State _state;
        private readonly SnippetSlashCommands _snippetSlashCommands;

        private const string SHOW_SNIPPET = "snippet";
        private const string LIST_SNIPPETS = "list-snippets";
        private const string SET_SNIPPET = "set-snippet";
        private const string REMOVE_SNIPPET = "remove-snippet";
        private const string EXPORT_SNIPPET = "export-snippet";

        private IList<Snippet> Snippets => _state.Snippets;

        [Command(SHOW_SNIPPET, RunMode = RunMode.Async), Name("Show snippet")]
        public async Task ShowSnippet(string prefix)
        {
            Snippets.GetSnippetEmbed(prefix, out var embed);
            await ReplyAsync(embed: embed.Build());
        }

        [Command(LIST_SNIPPETS, RunMode = RunMode.Async), Name("List snippets")]
        public async Task ListSnippets()
        {
            EmbedBuilder embed;
            if (Snippets.Any())
            {
                embed = new EmbedBuilder
                {
                    Color = Color.Teal,
                    Title = "Snippets"
                };

                foreach (var snippet in Snippets)
                {
                    embed.AddField(
                        Formatting.CODE_AFFIX + snippet.ID + Formatting.CODE_AFFIX,
                        "   " + snippet.Title
                    );
                }
            }
            else
            {
                embed = new EmbedBuilder
                {
                    Color = Color.Magenta,
                    Title = "Snippets",
                    Description = "No snippets have been created."
                };
            }

            await ReplyAsync(embed: embed.Build());
        }

        [Command(SET_SNIPPET, RunMode = RunMode.Async), Name("Create snippet"), RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetSnippet(string prefix, string title, [Remainder] string content)
        {
            if (Snippets.FirstOrDefault(s => s.ID == prefix) is Snippet store)
            {
                // Update the existing snippet
                store.Title = title;
                store.Content = content;
            }
            else
            {
                // Create a new snippet
                store = new Snippet(prefix, title, content);
                Snippets.Add(store);
            }

            _state.Write();
            _snippetSlashCommands.OnUpdate();

            var embed = new EmbedBuilder
            {
                Title = store.Title,
                Color = Color.Magenta,
                Description = store.Content
            };

            await ReplyAsync(embed: embed.Build());
        }

        [Command(REMOVE_SNIPPET, RunMode = RunMode.Async), Name("Delete snippet"), RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemoveSnippet(string prefix)
        {
            EmbedBuilder result;

            if (Snippets.FirstOrDefault(t => t.ID == prefix) is Snippet snippet)
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
                _state.Write();
                _snippetSlashCommands.OnUpdate();
            }
            else
            {
                result = new EmbedBuilder
                {
                    Color = Color.Red,
                    Title = "Failed to delete snippet",
                    Description = "The snippet was not found."
                };
            }

            await ReplyAsync(embed: result.Build());
        }

        [Command(EXPORT_SNIPPET, RunMode = RunMode.Async), Name("Export snippet")]
        public async Task ExportSnippet(string prefix)
        {
            await Context.Message!.DeleteAsync();

            if (Snippets.FirstOrDefault(s => s.ID == prefix) is Snippet snippet)
            {
                await ReplyAsync(Formatting.CodeBlock(snippet.Content));
            }
            else
            {
                var embed = new EmbedBuilder
                {
                    Color = Color.Red,
                    Title = "Failed to export snippet",
                    Description = "The snippet was not found."
                };

                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}