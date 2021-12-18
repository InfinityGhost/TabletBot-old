using System.Threading.Tasks;
using Discord;
using TabletBot.Common;

#nullable enable

namespace TabletBot.Discord.Watchers.Spam
{
    public class SpamMessageWatcher : IMessageWatcher
    {
        public async Task Receive(IMessage message)
        {
            if (_spamMessageList.Check(message))
            {
                var guildUser = message.Author as IGuildUser;
                await guildUser.BanAsync(1, "Automated ban for spam.");
                Log.Write("SpamDetect", $"User {message.Author.Username}#{message.Author.Discriminator} ({message.Author.Id}) was banned for spamming.");
            }
        }

        public Task Deleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel) => Task.CompletedTask;

        private readonly SpamMessageList _spamMessageList = new SpamMessageList();
    }
}