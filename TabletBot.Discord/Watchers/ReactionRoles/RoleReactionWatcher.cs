using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TabletBot.Common;
using TabletBot.Common.Store;
using static TabletBot.Discord.DiscordExtensions;

namespace TabletBot.Discord.Watchers.ReactionRoles
{
    public class RoleReactionWatcher : IReactionWatcher, IMessageWatcher
    {
        private readonly State _state;
        private readonly DiscordSocketClient _discordSocketClient;

        public RoleReactionWatcher(State state, DiscordSocketClient discordSocketClient)
        {
            _state = state;
            _discordSocketClient = discordSocketClient;
        }

        public async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            var textChannel = await channel.GetOrDownloadAsync() as ITextChannel;
            await HandleReactionAdded(textChannel!, reaction);
        }

        public async Task ReactionRemoved(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            var textChannel = await channel.GetOrDownloadAsync() as ITextChannel;
            await HandleReactionRemoved(textChannel!, reaction);
        }

        private async Task HandleReactionAdded(ITextChannel channel, SocketReaction reaction)
        {
            try
            {
                if (GetTrackedRole(reaction, _discordSocketClient) is RoleManagementMessage reactionRole)
                {
                    var guild = await _discordSocketClient.Rest.GetGuildAsync(channel.GuildId);
                    var role = guild.Roles.FirstOrDefault(r => r.Id == reactionRole!.RoleId);
                    var user = await guild.GetUserAsync(reaction.UserId);
                    await user.AddRoleAsync(role);
                }
            }
            catch (Exception ex)
            {
                var systemChannel = await channel.Guild.GetSystemChannelAsync();
                await ReplyException(systemChannel ?? channel, ex);
                Log.Exception(ex);
            }
        }

        private async Task HandleReactionRemoved(ITextChannel channel, SocketReaction reaction)
        {
            try
            {
                if (GetTrackedRole(reaction, _discordSocketClient) is RoleManagementMessage reactionRole)
                {
                    var guild = await _discordSocketClient.Rest.GetGuildAsync(channel.GuildId);
                    var role = guild.Roles.FirstOrDefault(r => r.Id == reactionRole!.RoleId);
                    var user = await guild.GetUserAsync(reaction.UserId);
                    await user.RemoveRoleAsync(role);
                }
            }
            catch (Exception ex)
            {
                var systemChannel = await channel.Guild.GetSystemChannelAsync();
                await ReplyException(systemChannel ?? channel, ex);
                Log.Exception(ex);
            }
        }

        public Task Receive(IMessage message) => Task.CompletedTask;

        public Task Deleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        {
            if (_state.ReactiveRoles.FirstOrDefault(m => m.MessageId == message.Id) is RoleManagementMessage roleStore)
                _state.ReactiveRoles.Remove(roleStore);

            return Task.CompletedTask;
        }

        private RoleManagementMessage? GetTrackedRole(
            SocketReaction reaction,
            BaseSocketClient client
        )
        {
            if (reaction.UserId == client.CurrentUser.Id)
                return default;

            var query = from reactRole in _state.ReactiveRoles
                where reactRole.MessageId == reaction.MessageId
                where reactRole.EmoteName == reaction.Emote.ToString()
                select reactRole;

            return query.FirstOrDefault();
        }
    }
}