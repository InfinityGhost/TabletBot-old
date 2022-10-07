using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TabletBot.Common;
using TabletBot.Discord.Commands;
using TabletBot.Discord.Watchers.Safe;

namespace TabletBot.Discord.Watchers.DirectMessage
{
    public class ModMailMessageWatcher : SafeMessageWatcher
    {
        private readonly DiscordSocketClient _client;
        private readonly Settings _settings;

        public ModMailMessageWatcher(DiscordSocketClient client, Settings settings)
        {
            _client = client;
            _settings = settings;
        }

        private ITextChannel? _directMessageLogChannel;

        protected override async Task ReceiveInternal(IMessage message)
        {
            if (message.Channel is IDMChannel dmChannel)
            {
                await DirectMessage(message, dmChannel);
            }
        }

        private async Task DirectMessage(IMessage message, IDMChannel dmChannel)
        {
            var embed = new EmbedBuilder
            {
                Description = message.Content,
                Timestamp = message.Timestamp,
                Author = dmChannel.Recipient.ToEmbedAuthor(),
                Color = Color.Blue,
                Fields = GetFieldForAttachments(message),
                Footer = new EmbedFooterBuilder
                {
                    Text = dmChannel.Id.ToString()
                }
            };

            var existingEmbeds = message.Embeds.Any() ? message.Embeds : Array.Empty<IEmbed>();

            var embeds = from emb in existingEmbeds.Prepend(embed.Build())
                where emb is Embed
                select emb as Embed;

            var channel = await GetModMailChannel();
            await channel.SendMessageAsync(embeds: embeds.ToArray());
        }

        private async Task<ITextChannel> GetModMailChannel()
        {
            if (_directMessageLogChannel != null)
                return _directMessageLogChannel;

            var channel = await _client.Rest.GetChannelAsync(_settings.ModMailChannelID);
            return _directMessageLogChannel = (ITextChannel) channel;
        }

        private static List<EmbedFieldBuilder> GetFieldForAttachments(IMessage message)
        {
            return message.Attachments.Select(GetFieldForAttachment).ToList();
        }

        private static EmbedFieldBuilder GetFieldForAttachment(IAttachment attachment)
        {
            return new EmbedFieldBuilder
            {
                Name = attachment.Filename,
                Value = Formatting.UrlString(attachment)
            };
        }
    }
}