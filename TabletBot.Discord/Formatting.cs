using System;
using System.Linq;
using System.Text;
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

        public static void AppendCodeBlock(this StringBuilder stringBuilder, string[] lines, string? lang = null)
        {
            TrimBaseIndentation(lines);

            stringBuilder.AppendLine(CODE_BLOCK + lang);
            foreach (var line in lines)
                stringBuilder.AppendLine(line);
            stringBuilder.AppendLine(CODE_BLOCK);
        }

        public static void TrimBaseIndentation(string[] lines)
        {
            var baseIndentationLength = lines.Min(line => {
                var indentation = CountIndentation(line);
                return line.Length > indentation ? indentation : int.MaxValue;
            });

            for (int i = 0; i != lines.Length; i++)
                lines[i] = lines[i].Substring(Math.Min(baseIndentationLength, lines[i].Length)).TrimEnd();
        }

        public static int CountIndentation(string line)
        {
            for (var i = 0; i < line.Length; i++)
                if (!char.IsWhiteSpace(line[i]))
                    return i;
            return int.MaxValue;
        }
    }
}
