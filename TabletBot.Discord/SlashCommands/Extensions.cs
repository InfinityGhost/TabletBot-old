using System.Linq;
using Discord.WebSocket;

namespace TabletBot.Discord.SlashCommands
{
    internal static class Extensions
    {
        public static T GetValue<T>(this SocketSlashCommand command, string option, T fallback = default(T))
        {
            var value = command.Data.Options?.FirstOrDefault(o => o.Name == option)?.Value;
            return value is T ? (T)value : fallback;
        }
    }
}