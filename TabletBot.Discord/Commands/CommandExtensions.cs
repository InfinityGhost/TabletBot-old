using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TabletBot.Common;
using TabletBot.Common.Store;

namespace TabletBot.Discord.Commands
{
    internal static class CommandExtensions
    {

        public static async Task Update(this IUserMessage message, Embed embed)
        {
            await message.ModifyAsync((msg) =>
            {
                msg.Content = null;
                msg.Embed = embed;
            });
        }

        public static Task Update(this IUserMessage message, EmbedBuilder embed) => Update(message, embed.Build());

        public static async Task Update(this IUserMessage message, string text)
        {
            await message.ModifyAsync((msg) =>
            {
                msg.Content = text;
                msg.Embed = null;
            });
        }

        public static async void DeleteDelayed(this IMessage message)
        {
            await Task.Delay(CommandModule.DeleteDelay);
            await message.DeleteAsync();
        }

        public static void EmbedParameters(this CommandInfo command, ref EmbedBuilder embed)
        {
            var field = new EmbedFieldBuilder
            {
                Name = string.Format("__{0}__", command.Name)
            };

            if (command.Summary != null)
                field.Value += command.Summary + Environment.NewLine;

            IEnumerable<string> parameters =
                from parameter in command.Parameters
                select string.Format((parameter.IsOptional ? "[{0}]" : "<{0}>"), parameter.Name);

            field.Value += string.Format("**Syntax**: `{0}{1}{2}`",
                Settings.Current.CommandPrefix,
                command.Aliases[0],
                (' ' + string.Join(" ", parameters)).TrimEnd());

            embed.AddField(field);
        }

        public static IEmote GetEmote(this string emoteName)
        {
            return Emote.TryParse(emoteName, out var emote) ? emote : new Emoji(emoteName);
        }
    }
}