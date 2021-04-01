using Discord.Commands;

namespace TabletBot.Discord.Commands
{
    public class CommandModule : ModuleBase
    {
        protected const string ITALIC_AFFIX = "*";
        protected const string BOLD_AFFIX = "**";
        protected const string UNDERLINE_AFFIX = "__";
        protected const string CODE_AFFIX = "`";

        protected const string QUOTE_PREFIX = "> ";
        protected const string CODE_BLOCK = "```";
    }
}