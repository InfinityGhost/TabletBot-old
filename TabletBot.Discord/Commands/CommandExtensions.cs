using System.Threading.Tasks;
using Discord;

namespace TabletBot.Discord.Commands
{
    internal static class CommandExtensions
    {
        public static async Task Update(this IUserMessage message, EmbedBuilder embed)
        {
            await message.ModifyAsync((msg) =>
            {
                msg.Content = null;
                msg.Embed = embed.Build();
            });
        }

        public static async Task Update(this IUserMessage message, string text)
        {
            await message.ModifyAsync((msg) =>
            {
                msg.Content = text;
                msg.Embed = null;
            });
        }

        public static async Task DeleteDelayed(this IMessage message)
        {
            await Task.Delay(CommandModule.DeleteDelay);
            await message.DeleteAsync();
        }
    }
}