using System.Threading.Tasks;
using Discord;

namespace TabletBot.Discord.Watchers
{
    public interface IMessageWatcher
    {
        Task Receive(IMessage message);
        Task Deleted(IMessage message);
    }
}