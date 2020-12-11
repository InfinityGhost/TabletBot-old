using System.Linq;
using TabletBot.Common;
using TabletBot.Discord;

namespace TabletBot
{
    internal static class Tools
    {
        public static bool TryGetRole(string args, out ulong roleId, out string name)
        {
            if (ulong.TryParse(args, out roleId))
            {
                name = null;
                return true;
            }
            else if (Bot.Current != null)
            {
                var role = Bot.Current.DiscordClient.GetGuild(Settings.Current.GuildID).Roles.First(r => r.Name.Contains(args[0]));
                name = role.Name;
                roleId = role.Id;
                return true;
            }
            else
            {
                roleId = 0;
                name = null;
                return false;
            }
        }
    }
}