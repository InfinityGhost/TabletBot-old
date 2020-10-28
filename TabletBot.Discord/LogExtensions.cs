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
                message.Content,
                message.Channel.Name,
                message.Author.Username,
                message.Author.Discriminator
            ), LogLevel.Message);
        }

        public static async Task WriteAsync(LogMessage message)
        {
            var logLevel = message.Severity switch
            {
                LogSeverity.Debug    => LogLevel.Debug,
                LogSeverity.Info     => LogLevel.Info,
                LogSeverity.Verbose  => LogLevel.Debug,
                LogSeverity.Warning  => LogLevel.Warn,
                LogSeverity.Error    => LogLevel.Error,
                LogSeverity.Critical => LogLevel.Fatal,
                _                    => (LogLevel)message.Severity
            };
            await Log.WriteAsync("Client", string.Format(
                "{1} [{2}]: {0}",
                message.Message,
                message.Source,
                message.Severity
            ), logLevel);
        }
    }
}