using System;
using System.Threading.Tasks;
using Discord;
using JetBrains.Annotations;
using TabletBot.Common;
using TabletBot.Discord.Embeds;

namespace TabletBot.Discord.Watchers.Safe
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public abstract class SafeMessageWatcher : IMessageWatcher
    {
        public async Task Receive(IMessage message)
        {
            try
            {
                await ReceiveInternal(message);
            }
            catch (Exception ex)
            {
                await HandleException(ex, message.Channel);
            }
        }

        public async Task Deleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        {
            try
            {
                await DeletedInternal(message, channel);
            }
            catch (Exception ex)
            {
                await HandleException(ex, message.HasValue ? message.Value.Channel : null);
            }
        }
        
        private static async Task HandleException(Exception ex, IMessageChannel? channel)
        {
            var embed = ExceptionEmbeds.GetEmbedForException(ex);
            Log.Exception(ex);
            
            if (channel != null)
                await channel.SendMessageAsync(embed: embed);
        }

        protected virtual Task ReceiveInternal(IMessage message) => Task.CompletedTask;
        protected virtual Task DeletedInternal(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel) => Task.CompletedTask;
    }
}