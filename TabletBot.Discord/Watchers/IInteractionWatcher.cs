using System.Threading.Tasks;
using Discord.WebSocket;

namespace TabletBot.Discord.Watchers
{
    public interface IInteractionWatcher
    {
        Task HandleInteraction(SocketInteraction interaction);
    }
}