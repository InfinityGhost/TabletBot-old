using System.Threading.Tasks;
using Discord;

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
                await guildUser.BanAsync();
            }
        }

        public Task Deleted(IMessage message) => Task.CompletedTask;

        private readonly SpamMessageList _spamMessageList = new SpamMessageList();
    }
}