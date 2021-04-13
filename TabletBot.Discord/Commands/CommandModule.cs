using System.Threading.Tasks;
using Discord.Commands;
using TabletBot.Common;

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

        protected async Task OverwriteSettings()
        {
            Platform.SettingsFile.Refresh();
            if (Platform.SettingsFile.Exists)
                await Settings.Current.Write(Platform.SettingsFile);
        }
    }
}