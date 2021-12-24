using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TabletBot.Common;
using TabletBot.Common.Store;
using TabletBot.Discord.Commands;
using TabletBot.Discord.Embeds;

namespace TabletBot.Discord
{
    internal static class DiscordExtensions
    {
        public static void DeleteAllDelayed(int delay, params IMessage[] messages)
        {
            try
            {
                Parallel.ForEach(messages, m => m.DeleteDelayed(delay));
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        public static async Task<IMessage> ReplyException(IMessageChannel channel, Exception ex)
        {
            var embed = ExceptionEmbeds.GetEmbedForException(ex);
            return await channel.SendMessageAsync(embed: embed);
        }
    }
}