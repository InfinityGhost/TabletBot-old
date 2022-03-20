using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TabletBot.Common;

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

        public static async void DeleteDelayed(this IMessage message, int delay)
        {
            if (delay > 0)
            {
                await Task.Delay(delay);
                await message.DeleteAsync();
            }
        }

        public static void EmbedParameters(this CommandInfo command, ref EmbedBuilder embed, Settings settings)
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
                settings.CommandPrefix,
                command.Aliases[0],
                (' ' + string.Join(" ", parameters)).TrimEnd());

            embed.AddField(field);
        }

        public static IEmote GetEmote(this string emoteName)
        {
            return Emote.TryParse(emoteName, out var emote) ? emote : new Emoji(emoteName);
        }

        public static EmbedAuthorBuilder ToEmbedAuthor(this IUser user)
        {
            return new EmbedAuthorBuilder
            {
                Name = user.Username,
                IconUrl = user.GetAvatarUrl()
            };
        }

        public static EmbedFooterBuilder ToEmbedFooter(this IUser user, string textFormat = "{0}")
        {
            return new EmbedFooterBuilder
            {
                Text = string.Format(user.Username, textFormat),
                IconUrl = user.GetAvatarUrl()
            };
        }

        public static MessageReference ToReference(this IUserMessage message)
        {
            if (message.Channel is IGuildChannel guildChannel)
                return new MessageReference(message.Id, message.Channel.Id, guildChannel.GuildId);

            return new MessageReference(message.Id, message.Channel.Id);
        }
    }
}