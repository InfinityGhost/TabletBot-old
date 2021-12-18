using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Discord;
using TabletBot.Common;

#nullable enable

namespace TabletBot.Discord.Watchers.Spam
{
    public class SpamMessageList : List<SpamWatcherItem>
    {
        public bool Check(IMessage message)
        {
            if (message.Author.IsBot || message.Channel is not IGuildChannel)
                return false;

            if (GetLastUserMessage(message.Author) is SpamWatcherItem item)
            {
                var since = item.Message.Timestamp - message.Timestamp;
                if (item.Message.CleanContent == message.CleanContent && since < TimeSpan.FromSeconds(30))
                {
                    item.Count++;
                }
                else
                {
                    item.Count = 1;
                    item.Message = message;
                }
                return item.Count >= Settings.Current.SpamThreshold;
            }

            var newItem = new SpamWatcherItem
            {
                Message = message
            };
            Add(newItem);

            return false;
        }

        private SpamWatcherItem? GetLastUserMessage(IUser user)
        {
            return this.FirstOrDefault(m => m.Message.Author.Id == user.Id);
        }
    }
}