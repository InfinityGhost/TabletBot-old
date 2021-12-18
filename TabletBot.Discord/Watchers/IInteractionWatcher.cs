using System.Threading.Tasks;
using Discord.WebSocket;

namespace TabletBot.Discord.Watchers
{
    public interface IInteractionWatcher : IWatcher
    {
        Task HandleInteraction(SocketInteraction interaction);
    }
}