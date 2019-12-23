using System.Threading.Tasks;
using Discord;

namespace TabletBot.Discord.Commands
{
    internal static class CommandExtensions
    {
        public static async Task Update(this IUserMessage message, EmbedBuilder embed)
        {
            await message.ModifyAsync((msg) => msg.Embed = embed.Build());
        }

        public static async Task Update(this IUserMessage message, string text)
        {
            await message.ModifyAsync((msg) => msg.Content = text);
        }
    }
}