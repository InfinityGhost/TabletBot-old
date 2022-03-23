using System.Linq;

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
            else if (Program.Bot != null)
            {
                var role = Program.DiscordClient!.GetGuild(Program.Settings.GuildID).Roles.First(r => r.Name.Contains(args![0]));
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