using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TabletBot.Common;
using TabletBot.Discord.Commands;

namespace TabletBot.Discord.Watchers.DirectMessage
{
    public class ModMailMessageWatcher : IMessageWatcher
    {
        private readonly DiscordSocketClient _client;
        private readonly Settings _settings;

        public ModMailMessageWatcher(DiscordSocketClient client, Settings settings)
        {
            _client = client;
            _settings = settings;
        }

        private IMessageChannel? _directMessageLogChannel;

        public async Task Receive(IMessage message)
        {
            if (message.Channel is IDMChannel dmChannel)
            {
                if (_directMessageLogChannel == null)
                {
                    var channel = await _client.Rest.GetChannelAsync(_settings.ModMailChannelID);
                    _directMessageLogChannel = (IMessageChannel) channel;
                }

                var embed = new EmbedBuilder
                {
                    Description = message.Content,
                    Timestamp = message.Timestamp,
                    Author = dmChannel.Recipient.ToEmbedAuthor(),
                    Color = Color.Blue
                };

                await _directMessageLogChannel.SendMessageAsync(embed: embed.Build());
            }
        }

        public Task Deleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel) => Task.CompletedTask;
    }
}