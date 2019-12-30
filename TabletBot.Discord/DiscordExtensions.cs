using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TabletBot.Discord.Commands;

namespace TabletBot.Discord
{
    internal static class DiscordExtensions
    {
        public static async void DeleteAllDelayed(params IMessage[] messages)
        {
            var tasks =
                from message in messages
                select message.DeleteDelayed();
            await Task.WhenAll(tasks);
        }

        public static void DeleteAllDelayed(this IList<IMessage> messages) => DeleteAllDelayed(messages);
    }
}