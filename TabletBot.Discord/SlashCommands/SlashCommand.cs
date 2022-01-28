using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace TabletBot.Discord.SlashCommands
{
    public class SlashCommand
    {
        public string Name { set; get; }
        public SlashCommandBuilder Builder { set; get; }
        public Func<SocketSlashCommand, Task> Handler { set; get; }
        public GuildPermissions? MinimumPermissions { set; get; }
        public bool Ephemeral { set; get; }

        public SlashCommandProperties Build() => Builder.Build();

        public async Task Invoke(SocketSlashCommand command)
        {
            await command.DeferAsync(Ephemeral);
            if (MinimumPermissions is GuildPermissions permissions)
            {
                var user = command.User as IGuildUser;
                if (HasCorrectPermissions(user))
                {
                    await Handler(command);
                }
                else
                {
                    await command.FollowupAsync("You do not have permissions to use this command.");
                }
            }
            else
            {
                await Handler(command);
            }
        }

        private bool HasCorrectPermissions(IGuildUser user)
        {
            var guildPermissions = MinimumPermissions.Value;
            var userPermissions = user.GuildPermissions;

            return userPermissions.Administrator ||
                guildPermissions.ToList().All(p => userPermissions.Has(p));
        }
    }
}