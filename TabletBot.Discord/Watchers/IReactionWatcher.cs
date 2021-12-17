using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace TabletBot.Discord.Watchers
{
    public interface IReactionWatcher
    {
        Task ReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction);
        Task ReactionRemoved(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction);
    }
}