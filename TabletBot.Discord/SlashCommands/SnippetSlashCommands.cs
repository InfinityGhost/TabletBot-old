using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TabletBot.Common;
using TabletBot.Common.Store;
using TabletBot.Discord.Embeds;

namespace TabletBot.Discord.SlashCommands
{
    public sealed class SnippetSlashCommands : SlashCommandModule
    {
        private readonly Settings _settings;

        public SnippetSlashCommands(Settings settings)
        {
            _settings = settings;
        }

        private const string SHOW_SNIPPET = "snippet";
        private const string SET_SNIPPET = "set-snippet";
        private const string REMOVE_SNIPPET = "remove-snippet";
        private const string EXPORT_SNIPPET = "export-snippet";

        private IList<SnippetStore> Snippets => _settings.Snippets;

        protected override IEnumerable<SlashCommand> GetSlashCommands()
        {
            var snippets = GetSnippets();

            yield return new SlashCommand
            {
                Name = SHOW_SNIPPET,
                Handler = ShowSnippet,
                Builder = new SlashCommandBuilder
                {
                    Name = SHOW_SNIPPET,
                    Description = "Shows a snippet",
                    Options = new List<SlashCommandOptionBuilder>
                    {
                        new SlashCommandOptionBuilder
                        {
                            Name = "snippet",
                            Description = "The name of the snippet to show",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = true,
                            Choices = snippets
                        }
                    }
                }
            };

            yield return new SlashCommand
            {
                Name = SET_SNIPPET,
                Handler = SetSnippet,
                MinimumPermissions = GuildPermissions.None.Modify(
                    sendTTSMessages: true
                ),
                Builder = new SlashCommandBuilder
                {
                    Name = SET_SNIPPET,
                    Description = "Sets a snippet",
                    Options = new List<SlashCommandOptionBuilder>
                    {
                        new SlashCommandOptionBuilder
                        {
                            Name = "snippet",
                            Description = "The name of the snippet to set",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = true
                        },
                        new SlashCommandOptionBuilder
                        {
                            Name = "title",
                            Description = "The title of the snippet",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = true
                        },
                        new SlashCommandOptionBuilder
                        {
                            Name = "content",
                            Description = "The content of the snippet",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = true
                        }
                    },
                },
            };

            yield return new SlashCommand
            {
                Name = REMOVE_SNIPPET,
                Handler = RemoveSnippet,
                MinimumPermissions = GuildPermissions.None.Modify(
                    sendTTSMessages: true
                ),
                Builder = new SlashCommandBuilder
                {
                    Name = REMOVE_SNIPPET,
                    Description = "Removes a snippet",
                    Options = new List<SlashCommandOptionBuilder>
                    {
                        new SlashCommandOptionBuilder
                        {
                            Name = "snippet",
                            Description = "The name of the snippet to remove",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = true,
                            Choices = snippets
                        }
                    }
                }
            };

            yield return new SlashCommand
            {
                Name = EXPORT_SNIPPET,
                Handler = ExportSnippet,
                Builder = new SlashCommandBuilder
                {
                    Name = EXPORT_SNIPPET,
                    Description = "Exports a snippet",
                    Options = new List<SlashCommandOptionBuilder>
                    {
                        new SlashCommandOptionBuilder
                        {
                            Name = "snippet",
                            Description = "The name of the snippet to export",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = true,
                            Choices = snippets
                        }
                    }
                }
            };
        }

        private async Task ShowSnippet(SocketSlashCommand command)
        {
            var snippet = command.GetValue<string>("snippet");

            if (SnippetEmbeds.GetSnippetEmbed(_settings, snippet, out var embed))
                await command.FollowupAsync(embed: embed.Build(), ephemeral: false);
            else
                await command.FollowupAsync("Could not find snippet");
        }

        private async Task SetSnippet(SocketSlashCommand command)
        {
            var snippet = command.GetValue<string>("snippet");
            var title = command.GetValue<string>("title");
            var content = command.GetValue<string>("content")?.Replace(@"\n", Environment.NewLine);

            if (Snippets.FirstOrDefault(s => s.Snippet == snippet) is SnippetStore store)
            {
                // Update the existing snippet
                store.Title = title;
                store.Content = content;
            }
            else
            {
                // Add a new snippet
                store = new SnippetStore(snippet, title, content);
                Snippets.Add(store);
            }

            await _settings.Overwrite();
            OnUpdate();
            await command.FollowupAsync(embed: new EmbedBuilder
            {
                Title = store.Title,
                Color = Color.Magenta,
                Description = store.Content
            }.Build());
        }

        private async Task RemoveSnippet(SocketSlashCommand command)
        {
            var snippet = command.GetValue<string>("snippet");

            if (Snippets.FirstOrDefault(t => t.Snippet == snippet) is SnippetStore store)
            {
                Snippets.Remove(store);
                var embed = new EmbedBuilder
                {
                    Color = Color.Green,
                    Title = "Successfully removed snippet",
                    Fields =
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Snippet",
                            Value = snippet
                        }
                    }
                };
                await _settings.Overwrite();
                OnUpdate();
                await command.FollowupAsync(embed: embed.Build());
            }
            else
            {
                await command.FollowupAsync("Could not find snippet");
            }
        }

        private async Task ExportSnippet(SocketSlashCommand command)
        {
            var snippet = command.GetValue<string>("snippet");

            if (Snippets.FirstOrDefault(s => s.Snippet == snippet) is SnippetStore store)
            {
                var sb = new StringBuilder();
                sb.AppendLine(Formatting.CodeString(store.Title));
                sb.AppendLine(Formatting.CodeBlock(store.Content.Replace(Environment.NewLine, @"\n")));

                await command.FollowupAsync(sb.ToString());
            }
        }

        private List<ApplicationCommandOptionChoiceProperties> GetSnippets()
        {
            if (!_settings.Snippets?.Any() ?? false)
            {
                return new List<ApplicationCommandOptionChoiceProperties>();
            }

            var query = from snippet in _settings.Snippets
                let option = new ApplicationCommandOptionChoiceProperties
                {
                    Name = $"{snippet.Snippet}: {snippet.Title}",
                    Value = snippet.Snippet
                }
                orderby option.Name
                select option;

            return query.ToList();
        }
    }
}