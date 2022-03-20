using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace TabletBot.Discord.SlashCommands
{
    public class ModerationSlashCommands : SlashCommandModule
    {
        private const string DELETE = "delete";
        private const string KICK_USER = "kick";
        private const string BAN_USER = "ban";
        private const string CREATE_EMBED = "embed";

        protected override IEnumerable<SlashCommand> GetSlashCommands()
        {
            yield return new SlashCommand
            {
                Name = DELETE,
                Handler = Delete,
                Ephemeral = true,
                MinimumPermissions = GuildPermissions.None.Modify(
                    manageMessages: true
                ),
                Builder = new SlashCommandBuilder
                {
                    Name = DELETE,
                    Description = "Deletes a message or a group of messages",
                    Options = new List<SlashCommandOptionBuilder>()
                    {
                        new SlashCommandOptionBuilder
                        {
                            Name = "amount",
                            Description = "The number of messages to delete (defaults to 1)",
                            Type = ApplicationCommandOptionType.Integer,
                            IsRequired = false
                        }
                    }
                }
            };

            yield return new SlashCommand
            {
                Name = KICK_USER,
                Handler = Kick,
                Ephemeral = true,
                MinimumPermissions = GuildPermissions.None.Modify(
                    kickMembers: true
                ),
                Builder = new SlashCommandBuilder
                {
                    Name = KICK_USER,
                    Description = "Kicks a user from the server",
                    Options = new List<SlashCommandOptionBuilder>()
                    {
                        new SlashCommandOptionBuilder
                        {
                            Name = "user",
                            Description = "The user to kick",
                            Type = ApplicationCommandOptionType.User,
                            IsRequired = true
                        },
                        new SlashCommandOptionBuilder
                        {
                            Name = "reason",
                            Description = "The reason for the kick",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = false
                        }
                    }
                }
            };

            yield return new SlashCommand
            {
                Name = BAN_USER,
                Handler = Ban,
                Ephemeral = true,
                MinimumPermissions = GuildPermissions.None.Modify(
                    banMembers: true
                ),
                Builder = new SlashCommandBuilder
                {
                    Name = BAN_USER,
                    Description = "Bans a user from the server",
                    Options = new List<SlashCommandOptionBuilder>()
                    {
                        new SlashCommandOptionBuilder
                        {
                            Name = "user",
                            Description = "The user to ban",
                            Type = ApplicationCommandOptionType.User,
                            IsRequired = true
                        },
                        new SlashCommandOptionBuilder
                        {
                            Name = "reason",
                            Description = "The reason for the ban",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = false
                        }
                    }
                }
            };

            yield return new SlashCommand
            {
                Name = CREATE_EMBED,
                Handler = CreateEmbed,
                Ephemeral = false,
                MinimumPermissions = GuildPermissions.None.Modify(
                    sendTTSMessages: true
                ),
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
                            IsRequired = false
                        },
                        new SlashCommandOptionBuilder
                        {
                            Name = "description",
                            Description = "The description of the embed",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = false
                        },
                        new SlashCommandOptionBuilder
                        {
                            Name = "color",
                            Description = "The color of the embed (hex)",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = false
                        },
                        new SlashCommandOptionBuilder
                        {
                            Name = "url",
                            Description = "The url of the embed",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = false
                        },
                        new SlashCommandOptionBuilder
                        {
                            Name = "footer",
                            Description = "The footer of the embed",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = false
                        },
                        new SlashCommandOptionBuilder
                        {
                            Name = "image",
                            Description = "The image URL to display in the embed",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = false
                        }
                    }
                }
            };
        }

        private async Task Delete(SocketSlashCommand command)
        {
            var amount = (int)command.GetValue<long>("amount", 1);

            var messages = await command.Channel.GetMessagesAsync(amount).FlattenAsync();
            await (command.Channel as ITextChannel).DeleteMessagesAsync(messages);
            await command.FollowupAsync($"Deleted {amount} messages.");
        }

        private async Task Kick(SocketSlashCommand command)
        {
            var user = command.GetValue<IGuildUser>("user");
            var reason = command.GetValue<string>("reason");
            
            if (user is IGuildUser)
            {
                await user.KickAsync(reason);
                if (reason != null)
                    await command.FollowupAsync($"Kicked {user.Mention} for \"{reason}\".");
                else
                    await command.FollowupAsync($"Kicked {user.Mention}.");
            }
            else
            {
                await command.FollowupAsync("This user is not a member of this guild.");
            }
        }

        private async Task Ban(SocketSlashCommand command)
        {
            var userId = command.GetValue<ulong>("user");
            var reason = command.GetValue<string>("reason");

            var user = await (command.Channel as IGuildChannel).GetUserAsync(userId);

            if (user is IGuildUser)
            {
                await user.BanAsync(reason: reason);
                if (reason != null)
                    await command.FollowupAsync($"Banned {user.Mention} for \"{reason}\".");
                else
                    await command.FollowupAsync($"Banned {user.Mention}.");
            }
            else
            {
                await command.FollowupAsync("This user is not a member of this guild.");
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
                embed = embed.WithDescription(description.Replace(@"\n", Environment.NewLine));
            if (color != null)
                embed = embed.WithColor(color.Value);
            if (url != null)
                embed = embed.WithUrl(url);
            if (footer != null)
                embed = embed.WithFooter(footer);
            if (image != null)
                embed = embed.WithImageUrl(image);
            
            await command.FollowupAsync(embed: embed.Build());
        }
    }
}