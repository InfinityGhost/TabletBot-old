using System;
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

        public static string UrlString(IAttachment attachment) => UrlString(attachment.Filename, attachment.Url);
        public static string UrlString(string text, string url) => $"[{text}]({url})";

        public static string CodeString(string text) => $"{CODE_AFFIX}{text}{CODE_AFFIX}";
        public static string CodeBlock(string text, string? lang = null) =>
            $"{CODE_BLOCK}{lang}{Environment.NewLine}{text}{Environment.NewLine}{CODE_BLOCK}";
    }
}