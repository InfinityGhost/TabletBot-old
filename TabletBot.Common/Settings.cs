using System.IO;

namespace TabletBot.Common
{
    public sealed class Settings : Serializable
    {
        private const ulong MAIN_GUILD_ID = 615607687467761684;
        private const ulong LOG_MESSAGE_CHANNEL_ID = 715344685853442198;
        private const ulong MOD_MAIL_CHANNEL_ID = 958916136966193182;
        private const ulong MODERATOR_ROLE_ID = 644180151755735060;
        private const ulong MUTED_ROLE_ID = 715342682293010452;

        public ulong GuildID { set; get; } = MAIN_GUILD_ID;
        public ulong LogMessageChannelID { set; get; } = LOG_MESSAGE_CHANNEL_ID;
        public ulong ModMailChannelID { set; get; } = MOD_MAIL_CHANNEL_ID;
        public ulong ModeratorRoleID { set; get; } = MODERATOR_ROLE_ID;
        public ulong MutedRoleID { set; get; } = MUTED_ROLE_ID;
        public string CommandPrefix { set; get; } = "!";
        public uint GitHubIssueRefLimit { set; get; } = 3;
        public uint SpamThreshold { set; get; } = 2;

        public LogLevel LogLevel { set; get; } = LogLevel.Debug;

        public override FileInfo File { get; } = AppData.SettingsFile;
    }
}
