using System;
using System.Threading.Tasks;
using Discord;
using Message = TabletBot.Discord.Common.LogMessage;

namespace TabletBot.Discord
{
    public static class Log
    {
        public static event EventHandler<Message> Output;

        public static void Write(string group, string text)
        {
            Output?.Invoke(null, new Message(group, text));
        }

        public static async Task WriteAsync(string group, string text) => await Task.Run(() => Write(group, text));

        public static void Debug(string text)
        {
            Write("DEBUG", text);
        }

        public static void Exception(Exception exception)
        {
            Write(exception.GetType().Name, exception.Message);
        }

        public static async Task WriteAsync(IMessage message)
        {
            await Log.WriteAsync("Message", string.Format(
                "{1}/{2}#{3}: {0}",
                message.Content,
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