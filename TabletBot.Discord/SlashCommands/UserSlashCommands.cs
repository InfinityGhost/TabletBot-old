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
                            IsRequired = false
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
    }
}