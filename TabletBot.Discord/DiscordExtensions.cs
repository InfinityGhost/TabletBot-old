using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TabletBot.Common;
using TabletBot.Discord.Commands;

namespace TabletBot.Discord
{
    internal static class DiscordExtensions
    {
        public static async void DeleteAllDelayed(params IMessage[] messages)
        {
            try
            {
                var tasks = from message in messages
                    select message.DeleteDelayed();
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        public static void DeleteAllDelayed(this IList<IMessage> messages) => DeleteAllDelayed(messages);
    }
}