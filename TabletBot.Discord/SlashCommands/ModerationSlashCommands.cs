using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Octokit;
using TabletBot.Common;

namespace TabletBot.Discord.SlashCommands
{
    public class ModerationSlashCommands : SlashCommandModule
    {
        protected const string DELETE = "delete";
        protected const string KICK_USER = "kick";
        protected const string BAN_USER = "ban";

        protected override IEnumerable<SlashCommand> GetSlashCommands()
        {
            yield return new SlashCommand
            {
                Name = DELETE,
                Handler = Delete,
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
                            Required = false
                        }
                    }
                }
            };

            yield return new SlashCommand
            {
                Name = KICK_USER,
                Handler = Kick,
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
                            Required = true
                        },
                        new SlashCommandOptionBuilder
                        {
                            Name = "reason",
                            Description = "The reason for the kick",
                            Type = ApplicationCommandOptionType.String,
                            Required = false
                        }
                    }
                }
            };

            yield return new SlashCommand
            {
                Name = BAN_USER,
                Handler = Ban,
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
                            Required = true
                        },
                        new SlashCommandOptionBuilder
                        {
                            Name = "reason",
                            Description = "The reason for the ban",
                            Type = ApplicationCommandOptionType.String,
                            Required = false
                        }
                    }
                }
            };
        }

        private async Task Delete(SocketSlashCommand command)
        {
            var amount = command.GetValue<int>("amount", 1);

            var messages = await command.Channel.GetMessagesAsync(amount).FlattenAsync();
            await (command.Channel as ITextChannel).DeleteMessagesAsync(messages);
            await command.RespondAsync($"Deleted {amount} messages.", ephemeral: true);
        }

        private async Task Kick(SocketSlashCommand command)
        {
            var user = command.GetValue<IGuildUser>("user");
            var reason = command.GetValue<string>("reason");
            
            if (user is IGuildUser)
            {
                await user.KickAsync(reason);
                if (reason != null)
                    await command.RespondAsync($"Kicked {user.Mention} for \"{reason}\".", ephemeral: true);
                else
                    await command.RespondAsync($"Kicked {user.Mention}.", ephemeral: true);
            }
            else
            {
                await command.RespondAsync(
                    $"This user is not a member of this guild.",
                    ephemeral: true
                );
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
                    await command.RespondAsync($"Banned {user.Mention} for \"{reason}\".", ephemeral: true);
                else
                    await command.RespondAsync($"Banned {user.Mention}.", ephemeral: true);
            }
            else
            {
                await command.RespondAsync(
                    $"This user is not a member of this guild.",
                    ephemeral: true
                );
            }
        }
    }
}