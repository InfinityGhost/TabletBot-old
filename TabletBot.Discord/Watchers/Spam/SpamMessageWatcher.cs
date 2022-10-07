using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using TabletBot.Common;
using TabletBot.Discord.Watchers.Safe;

namespace TabletBot.Discord.Watchers.Spam
{
    public class SpamMessageWatcher : SafeMessageWatcher
    {
        private readonly Settings _settings;

        public SpamMessageWatcher(Settings settings)
        {
            _settings = settings;
        }

        protected override async Task ReceiveInternal(IMessage message)
        {
            if(!_spamMessageLists.ContainsKey(message.Author.Id))
                _spamMessageLists.Add(message.Author.Id, new SpamMessageList(_settings.SpamThreshold));

            var spamMessageList = _spamMessageLists[message.Author.Id];
            if (spamMessageList.Check(message))
            {
                var guildUser = message.Author as IGuildUser;
                var guild = guildUser!.Guild;

                var mutedRole = guild.GetRole(_settings.MutedRoleID);
                await guildUser.AddRoleAsync(mutedRole);
                await Task.WhenAll(spamMessageList.Select(m => m.DeleteAsync()));

                var logMessage = $"User {message.Author.Username}#{message.Author.Discriminator} ({message.Author.Id}) was muted for spamming.";
                var logChannel = await guild.GetTextChannelAsync(_settings.LogMessageChannelID);
                await logChannel.SendMessageAsync(logMessage);
                Log.Write("SpamDetect", logMessage);
            }
        }

        private readonly IDictionary<ulong, SpamMessageList> _spamMessageLists = new Dictionary<ulong, SpamMessageList>();
    }
}