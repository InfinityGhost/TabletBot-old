using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using TabletBot.Common;

#nullable enable

namespace TabletBot.Discord.Watchers.Spam
{
    public class SpamMessageList : List<IMessage>
    {
        private readonly uint _spamThreshold;

        public SpamMessageList(uint spamThreshold)
        {
            _spamThreshold = spamThreshold;
        }

        public bool Check(IMessage message)
        {
            if (message.Author.IsBot || message.Channel is not IGuildChannel)
                return false;

            var lastMessage = this.LastOrDefault();
            if (lastMessage == null)
            {
                Add(message);
                return false;
            }

            bool withinTime = lastMessage.Timestamp - message.Timestamp < TimeSpan.FromSeconds(30);
            bool contentMatches = message.CleanContent != null && message.CleanContent == lastMessage.CleanContent;
            bool containsUrl = ContainsUrl(message);

            if (withinTime && contentMatches && containsUrl)
            {
                Add(message);
                return this.GroupBy(msg => msg.Channel).Count() > _spamThreshold;
            }

            Clear();
            Add(message);
            return false;
        }

        private bool ContainsUrl(IMessage message)
        {
            return message.Embeds.Any(embed => embed.Type == EmbedType.Link) || ContainsUrl(message.CleanContent);
        }

        private bool ContainsUrl(string? message)
        {
            if (message == null)
                return false;

            return message.Contains("http://") || message.Contains("https://");
        }
    }
}