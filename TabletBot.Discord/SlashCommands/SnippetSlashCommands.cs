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
        private const string SHOW_SNIPPET = "snippet";
        private const string SET_SNIPPET = "set-snippet";
        private const string REMOVE_SNIPPET = "remove-snippet";
        private const string EXPORT_SNIPPET = "export-snippet";

        private static IList<SnippetStore> Snippets => Settings.Current.Snippets;

        protected override IEnumerable<SlashCommand> GetSlashCommands()
        {
            yield return new SlashCommand
            {
                Name = SHOW_SNIPPET,
                Handler = ShowSnippet,
                Builder = new SlashCommandBuilder
                {
                    Name = SHOW_SNIPPET,
                    Description = "Shows a snippet",
                    Options = new List<SlashCommandOptionBuilder>()
                    {
                        new SlashCommandOptionBuilder
                        {
                            Name = "snippet",
                            Description = "The name of the snippet to show",
                            Type = ApplicationCommandOptionType.String,
                            Required = true,
                            Choices = GetSnippets()
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
                    Options = new List<SlashCommandOptionBuilder>()
                    {
                        new SlashCommandOptionBuilder
                        {
                            Name = "snippet",
                            Description = "The name of the snippet to set",
                            Type = ApplicationCommandOptionType.String,
                            Required = true
                        },
                        new SlashCommandOptionBuilder
                        {
                            Name = "title",
                            Description = "The title of the snippet",
                            Type = ApplicationCommandOptionType.String,
                            Required = true
                        },
                        new SlashCommandOptionBuilder
                        {
                            Name = "content",
                            Description = "The content of the snippet",
                            Type = ApplicationCommandOptionType.String,
                            Required = true
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
                    Options = new List<SlashCommandOptionBuilder>()
                    {
                        new SlashCommandOptionBuilder
                        {
                            Name = "snippet",
                            Description = "The name of the snippet to remove",
                            Type = ApplicationCommandOptionType.String,
                            Required = true,
                            Choices = GetSnippets()
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
                    Options = new List<SlashCommandOptionBuilder>()
                    {
                        new SlashCommandOptionBuilder
                        {
                            Name = "snippet",
                            Description = "The name of the snippet to export",
                            Type = ApplicationCommandOptionType.String,
                            Required = true,
                            Choices = GetSnippets()
                        }
                    }
                }
            };
        }

        private static async Task ShowSnippet(SocketSlashCommand command)
        {
            var snippet = GetValue<string>(command, "snippet");

            if (SnippetEmbeds.TryGetSnippetEmbed(snippet, out var embed))
                await command.RespondAsync(embed: embed.Build());
            else
                await command.RespondAsync("Could not find snippet");
        }

        private static async Task SetSnippet(SocketSlashCommand command)
        {
            var snippet = GetValue<string>(command, "snippet");
            var title = GetValue<string>(command, "title");
            var content = GetValue<string>(command, "content");

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

            await Settings.Current.Overwrite();
            await Bot.Current.UpdateSlashCommands();
            await command.RespondAsync(embed: SnippetEmbeds.GetSnippetEmbed(store).Build());
        }

        private static async Task RemoveSnippet(SocketSlashCommand command)
        {
            var snippet = GetValue<string>(command, "snippet");

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
                            Name = "Prefix",
                            Value = snippet
                        }
                    }
                };
                await Settings.Current.Overwrite();
                await Bot.Current.UpdateSlashCommands();
                await command.RespondAsync(embed: embed.Build());
            }
            else
            {
                await command.RespondAsync("Could not find snippet");
            }
        }

        private static async Task ExportSnippet(SocketSlashCommand command)
        {
            var snippet = GetValue<string>(command, "snippet");

            if (Snippets.FirstOrDefault(s => s.Snippet == snippet) is SnippetStore store)
            {
                var sb = new StringBuilder();
                sb.AppendLine(Formatting.CODE_BLOCK);
                sb.AppendLine(store.Content);
                sb.AppendLine(Formatting.CODE_BLOCK);

                await command.RespondAsync(sb.ToString());
            }
        }

        private static List<ApplicationCommandOptionChoiceProperties> GetSnippets()
        {
            if (!Settings.Current.Snippets?.Any() ?? false)
            {
                return new List<ApplicationCommandOptionChoiceProperties>();
            }

            return Settings.Current.Snippets.Select(s =>
                new ApplicationCommandOptionChoiceProperties
                {
                    Name = s.Title,
                    Value = s.Snippet
                }
            ).ToList();
        }
    }
}