using Discord;

namespace TabletBot.Discord.Watchers.Spam
{
    public class SpamWatcherItem
    {
        public IMessage Message { set; get; }
        public uint Count { set; get; }
    }
}