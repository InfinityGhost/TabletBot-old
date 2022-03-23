using System.Threading.Tasks;
using Discord;
using TabletBot.Common;
using LogMessage = Discord.LogMessage;

namespace TabletBot.Discord
{
    public static class LogExtensions
    {
        public static async Task WriteAsync(IMessage message)
        {
            await Log.WriteAsync("Message", string.Format(
                "#{1}/{2}#{3}: {0}",
                message.CleanContent,
                message.Channel.Name,
                message.Author.Username,
                message.Author.Discriminator
            ));
        }

        public static async Task WriteAsync(LogMessage message)
        {
            await Log.WriteAsync("Client", string.Format(
                "{1} [{2}]: {0}",
                message.Message,
                message.Source,
                message.Severity
            ));
        }
    }
}