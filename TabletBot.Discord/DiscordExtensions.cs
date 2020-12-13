using System;
using System.Collections.Generic;
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
        public static void DeleteAllDelayed(params IMessage[] messages)
        {
            try
            {
                Parallel.ForEach(messages, m => m.DeleteDelayed());
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        public static void DeleteAllDelayed(this IList<IMessage> messages) => DeleteAllDelayed(messages);

        public static bool IsTracked(
            this SocketReaction reaction,
            out RoleManagementMessageStore reactionRole
        )
        {
            if (reaction.UserId == Bot.Current.DiscordClient.CurrentUser.Id)
            {
                reactionRole = default;
                return false;
            }

            var query = from reactRole in Settings.Current.ReactiveRoles
                where reactRole.MessageId == reaction.MessageId
                where reactRole.EmoteName == reaction.Emote.ToString()
                select reactRole;
            reactionRole = query.FirstOrDefault();
            return reactionRole != null;
        }

        public static async Task<IMessage> ReplyException(IMessageChannel channel, Exception ex)
        {
            var embed = ExceptionEmbeds.GetEmbedForException(ex);
            return await channel.SendMessageAsync(embed: embed);
        }
    }
}