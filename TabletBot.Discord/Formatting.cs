using Discord;

namespace TabletBot.Discord
{
    public static class Formatting
    {
        public const string ITALIC_AFFIX = "*";
        public const string BOLD_AFFIX = "**";
        public const string UNDERLINE_AFFIX = "__";
        public const string CODE_AFFIX = "`";
        public const string QUOTE_PREFIX = "> ";
        public const string CODE_BLOCK = "```";

        public static string UrlString(IAttachment attachment) => $"[{attachment.Filename}]({attachment.Url})";
    }
}