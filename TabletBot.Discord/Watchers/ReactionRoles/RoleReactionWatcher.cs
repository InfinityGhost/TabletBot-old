using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TabletBot.Common;
using TabletBot.Discord.Commands;
using static TabletBot.Discord.DiscordExtensions;

namespace TabletBot.Discord.Watchers.ReactionRoles
{
    public class RoleReactionWatcher : IReactionWatcher
    {
        private DiscordSocketClient _discordSocketClient;

        public RoleReactionWatcher(DiscordSocketClient discordSocketClient)
        {
            _discordSocketClient = discordSocketClient;
        }

        public async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            var textChannel = await channel.GetOrDownloadAsync() as ITextChannel;
            await HandleReactionAdded(textChannel, reaction);
        }

        public async Task ReactionRemoved(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            var textChannel = await channel.GetOrDownloadAsync() as ITextChannel;
            await HandleReactionRemoved(textChannel, reaction);
        }

        private async Task HandleReactionAdded(ITextChannel channel, SocketReaction reaction)
        {
            try
            {
                if (reaction.IsTracked(out var reactionRole))
                {
                    var guild = await _discordSocketClient.Rest.GetGuildAsync(channel.GuildId);
                    var role = guild.Roles.FirstOrDefault(r => r.Id == reactionRole.RoleId);
                    var user = await guild.GetUserAsync(reaction.UserId);
                    await user.AddRoleAsync(role);
                }
            }
            catch (Exception ex)
            {
                var systemChannel = await channel.Guild.GetSystemChannelAsync();
                var reply = await ReplyException(systemChannel ?? channel, ex);
                reply.DeleteDelayed();
                Log.Exception(ex);
            }
        }

        private async Task HandleReactionRemoved(ITextChannel channel, SocketReaction reaction)
        {
            try
            {
                if (reaction.IsTracked(out var reactionRole))
                {
                    var guild = await _discordSocketClient.Rest.GetGuildAsync(channel.GuildId);
                    var role = guild.Roles.FirstOrDefault(r => r.Id == reactionRole.RoleId);
                    var user = await guild.GetUserAsync(reaction.UserId);
                    await user.RemoveRoleAsync(role);
                }
            }
            catch (Exception ex)
            {
                var systemChannel = await channel.Guild.GetSystemChannelAsync();
                var reply = await ReplyException(systemChannel ?? channel, ex);
                reply.DeleteDelayed();
                Log.Exception(ex);
            }
        }
    }
}