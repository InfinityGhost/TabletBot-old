using System.Threading.Tasks;
using Discord;

namespace TabletBot.Discord.Watchers
{
    public interface IMessageWatcher : IWatcher
    {
        Task Receive(IMessage message);
        Task Deleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel);
    }
}