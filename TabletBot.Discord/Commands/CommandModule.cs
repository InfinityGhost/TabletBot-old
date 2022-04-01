using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using JetBrains.Annotations;

namespace TabletBot.Discord.Commands
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public class CommandModule : ModuleBase
    {
        protected override Task<IUserMessage> ReplyAsync(
            string? message = null,
            bool isTTS = false,
            Embed? embed = null,
            RequestOptions? options = null,
            AllowedMentions? allowedMentions = null,
            MessageReference? messageReference = null,
            MessageComponent? components = null,
            ISticker[]? stickers = null,
            Embed[]? embeds = null
        )
        {
            messageReference ??= Context.Message.ToReference();

            return base.ReplyAsync(message, isTTS, embed, options, allowedMentions, messageReference, components,
                stickers, embeds);
        }
    }
}