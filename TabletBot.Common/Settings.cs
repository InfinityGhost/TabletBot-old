using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TabletBot.Common.Store;

namespace TabletBot.Common
{
    public sealed class Settings
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
        public uint SpamThreshold { set; get; } = 3;

        public LogLevel LogLevel { set; get; } = LogLevel.Debug;

        public Collection<RoleManagementMessageStore> ReactiveRoles { set; get; } = new Collection<RoleManagementMessageStore>();
        public Collection<SnippetStore> Snippets { set; get; } = new Collection<SnippetStore>();

        [JsonIgnore]
        public bool RunAsUnit { set; get; } = false;

        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public async Task Write(FileInfo file)
        {
            if (file.Directory is { Exists: false })
                file.Directory.Create();
            await using (var fs = file.Create())
                await JsonSerializer.SerializeAsync(fs, this, SerializerOptions);
        }

        public static async Task<Settings> Read(FileInfo file)
        {
            await using (var fs = file.OpenRead())
                return await JsonSerializer.DeserializeAsync<Settings>(fs);
        }

        public async Task<string> ExportAsync()
        {
            await using (var ms = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(ms, this, SerializerOptions);
                ms.Position = 0;
                using (var sr = new StreamReader(ms))
                    return await sr.ReadToEndAsync();
            }
        }

        public async Task Overwrite()
        {
            Platform.SettingsFile.Refresh();
            if (Platform.SettingsFile.Exists)
                await Write(Platform.SettingsFile);
        }
    }
}