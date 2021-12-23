using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TabletBot.Common;
using TabletBot.Common.Store;
using TabletBot.Discord.Embeds;
using TabletBot.Discord.SlashCommands;
using TabletBot.Discord.Watchers;

namespace TabletBot.Discord.Commands
{
    public class SnippetCommands : CommandModule
    {
        public SnippetCommands(Settings settings, IEnumerable<SlashCommandModule> slashCommands)
        {
            _settings = settings;
            _snippetSlashCommands = slashCommands.FirstOfType<SlashCommandModule, SnippetSlashCommands>();
        }

        private readonly Settings _settings;
        private readonly SnippetSlashCommands _snippetSlashCommands;

        private const string SHOW_SNIPPET = "snippet";
        private const string LIST_SNIPPETS = "list-snippets";
        private const string SET_SNIPPET = "set-snippet";
        private const string REMOVE_SNIPPET = "remove-snippet";
        private const string EXPORT_SNIPPET = "export-snippet";

        private IList<SnippetStore> Snippets => _settings.Snippets;

        [Command(SHOW_SNIPPET, RunMode = RunMode.Async), Name("Show snippet")]
        public async Task ShowSnippet(string prefix)
        {
            await Context.Message!.DeleteAsync();

            if (SnippetEmbeds.TryGetSnippetEmbed(_settings, prefix, out var embed))
            {
                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                var message = await ReplyAsync(embed: embed.Build(), messageReference: Context.Message.Reference);
                message.DeleteDelayed(_settings.DeleteDelay);
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
                        Formatting.CODE_AFFIX + snippet.Snippet + Formatting.CODE_AFFIX,
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
                message.DeleteDelayed(_settings.DeleteDelay);
            }
        }

        [Command(SET_SNIPPET, RunMode = RunMode.Async), Name("Create snippet"), RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetSnippet(string prefix, string title, [Remainder] string content)
        {
            await Context.Message!.DeleteAsync();

            if (Snippets.FirstOrDefault(s => s.Snippet == prefix) is SnippetStore store)
            {
                // Update the existing snippet
                store.Title = title;
                store.Content = content;
            }
            else
            {
                // Create a new snippet
                store = new SnippetStore(prefix, title, content);
                Snippets.Add(store);
            }
            await _settings.Overwrite();
            _snippetSlashCommands.OnUpdate();
            await ReplyAsync(embed: SnippetEmbeds.GetSnippetEmbed(store).Build());
        }

        [Command(REMOVE_SNIPPET, RunMode = RunMode.Async), Name("Delete snippet"), RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemoveSnippet(string prefix)
        {
            await Context.Message!.DeleteAsync();

            EmbedBuilder result;

            if (Snippets.FirstOrDefault(t => t.Snippet == prefix) is SnippetStore snippet)
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
                await _settings.Overwrite();
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

            var message = await ReplyAsync(embed: result.Build());
            message.DeleteDelayed(_settings.DeleteDelay);
        }

        [Command(EXPORT_SNIPPET, RunMode = RunMode.Async), Name("Export snippet")]
        public async Task ExportSnippet(string prefix)
        {
            await Context.Message!.DeleteAsync();

            if (Snippets.FirstOrDefault(s => s.Snippet == prefix) is SnippetStore snippet)
            {
                var sb = new StringBuilder();
                sb.AppendLine(Formatting.CODE_BLOCK);
                sb.AppendLine(snippet.Content);
                sb.AppendLine(Formatting.CODE_BLOCK);

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
                message.DeleteDelayed(_settings.DeleteDelay);
            }
        }
    }
}