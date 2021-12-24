using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TabletBot.Common;

#nullable enable

namespace TabletBot.Discord.Watchers.Spam
{
    public class SpamMessageWatcher : IMessageWatcher
    {
        private readonly Settings _settings;
        private readonly DiscordSocketClient _discordClient;

        public SpamMessageWatcher(Settings settings, DiscordSocketClient discordClient)
        {
            _settings = settings;
            _discordClient = discordClient;
        }

        public async Task Receive(IMessage message)
        {
            if(!_spamMessageLists.ContainsKey(message.Author.Id))
                _spamMessageLists.Add(message.Author.Id, new SpamMessageList(_settings.SpamThreshold));

            var spamMessageList = _spamMessageLists[message.Author.Id];
            if (spamMessageList.Check(message))
            {
                var guildUser = message.Author as IGuildUser;
                var guild = _discordClient.Guilds.First(g => g.Id == _settings.GuildID);
                var mutedRole = guild.GetRole(_settings.MutedRoleID);
                await guildUser!.AddRoleAsync(mutedRole);
                await Task.WhenAll(spamMessageList.Select(m => m.DeleteAsync()));

                var logMessage = $"User {message.Author.Username}#{message.Author.Discriminator} ({message.Author.Id}) was muted for spamming.";
                var logChannel = guild.GetTextChannel(_settings.LogMessageChannelID);
                await logChannel.SendMessageAsync(logMessage);
                Log.Write("SpamDetect", logMessage);
            }
        }

        public Task Deleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel) => Task.CompletedTask;

        private readonly IDictionary<ulong, SpamMessageList> _spamMessageLists = new Dictionary<ulong, SpamMessageList>();
    }
}