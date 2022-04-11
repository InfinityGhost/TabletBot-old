using System.Threading.Tasks;
using Discord;
using TabletBot.Common;
using LogMessage = Discord.LogMessage;

namespace TabletBot.Discord
{
    public static class LogExtensions
    {
        public static Task WriteAsync(IMessage message)
        {
            Log.Write("Message", string.Format(
                "#{1}/{2}#{3}: {0}",
                message.CleanContent,
                message.Channel.Name,
                message.Author.Username,
                message.Author.Discriminator
            ));

            return Task.CompletedTask;
        }

        public static Task WriteAsync(LogMessage message)
        {
            Log.Write("Client", string.Format(
                "{1} [{2}]: {0}",
                message.Message,
                message.Source,
                message.Severity
            ));

            return Task.CompletedTask;
        }
    }
}