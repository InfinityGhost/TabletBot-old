using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace TabletBot.Discord.SlashCommands
{
    public class UserSlashCommands : SlashCommandModule
    {
        protected const string SET_TABLET = "tablet";
        protected const string CREATE_EMBED = "create-embed";

        protected override IEnumerable<SlashCommand> GetSlashCommands()
        {
            yield return new SlashCommand
            {
                Name = SET_TABLET,
                Handler = SetTablet,
                Builder = new SlashCommandBuilder
                {
                    Name = SET_TABLET,
                    Description = "Adds your tablet to your nickname.",
                    Options = new List<SlashCommandOptionBuilder>()
                    {
                        new SlashCommandOptionBuilder
                        {
                            Name = "tablet",
                            Description = "The name of the tablet you want to add to your nickname",
                            Type = ApplicationCommandOptionType.String,
                            Required = false
                        }
                    }
                }
            };

            yield return new SlashCommand
            {
                Name = CREATE_EMBED,
                Handler = CreateEmbed,
                Builder = new SlashCommandBuilder
                {
                    Name = CREATE_EMBED,
                    Description = "Creates an embed to send to the channel.",
                    Options = new List<SlashCommandOptionBuilder>()
                    {
                        new SlashCommandOptionBuilder
                        {
                            Name = "title",
                            Description = "The title of the embed",
                            Type = ApplicationCommandOptionType.String,
                            Required = false
                        },
                        new SlashCommandOptionBuilder
                        {
                            Name = "description",
                            Description = "The description of the embed",
                            Type = ApplicationCommandOptionType.String,
                            Required = false
                        },
                        new SlashCommandOptionBuilder
                        {
                            Name = "color",
                            Description = "The color of the embed (hex)",
                            Type = ApplicationCommandOptionType.String,
                            Required = false
                        },
                        new SlashCommandOptionBuilder
                        {
                            Name = "url",
                            Description = "The url of the embed",
                            Type = ApplicationCommandOptionType.String,
                            Required = false
                        },
                        new SlashCommandOptionBuilder
                        {
                            Name = "footer",
                            Description = "The footer of the embed",
                            Type = ApplicationCommandOptionType.String,
                            Required = false
                        },
                        new SlashCommandOptionBuilder
                        {
                            Name = "image",
                            Description = "The image URL to display in the embed",
                            Type = ApplicationCommandOptionType.String,
                            Required = false
                        }
                    }
                }
            };
        }

        private async Task SetTablet(SocketSlashCommand command)
        {
            var tablet = command.GetValue<string>("tablet");
            var user = command.User as IGuildUser;
            
            if (tablet != null)
            {
                await user.ModifyAsync(u => u.Nickname = $"{user.Username} | {tablet}");
                await command.RespondAsync($"Your nickname has updated to include your tablet.", ephemeral: true);
            }
            else
            {
                await user.ModifyAsync(u => u.Nickname = null);
                await command.RespondAsync($"Your nickname has been reset.", ephemeral: true);
            }
        }

        private async Task CreateEmbed(SocketSlashCommand command)
        {
            var title = command.GetValue<string>("title");
            var description = command.GetValue<string>("description");
            var colorHex = command.GetValue<string>("color");
            var url = command.GetValue<string>("url");
            var footer = command.GetValue<string>("footer");
            var image = command.GetValue<string>("image");

            var color = colorHex != null ? (Color?)System.Drawing.ColorTranslator.FromHtml(colorHex) : (Color?)null;

            var embed = new EmbedBuilder();
            if (title != null)
                embed = embed.WithTitle(title);
            if (description != null)
                embed = embed.WithDescription(description);
            if (color != null)
                embed = embed.WithColor(color.Value);
            if (url != null)
                embed = embed.WithUrl(url);
            if (footer != null)
                embed = embed.WithFooter(footer);
            if (image != null)
                embed = embed.WithImageUrl(image);
            
            await command.RespondAsync(embed: embed.Build());
        }
    }
}