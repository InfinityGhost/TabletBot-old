using System;
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
        public const ulong MAIN_GUILD_ID = 615607687467761684;
        public const ulong MODERATOR_ROLE_ID = 644180151755735060;

        public static Settings Current { set; get; } = new Settings();

        public int DeleteDelay { set; get; } = 5000;
        public ulong GuildID { set; get; } = MAIN_GUILD_ID;
        public ulong ModeratorRoleID { set; get; } = MODERATOR_ROLE_ID;
        public string DiscordBotToken { set; get; } = null;
        public string GitHubToken { set; get; } = null;
        public string CommandPrefix { set; get; } = "!";
        public uint GitHubIssueRefLimit { set; get; } = 3;

        public LogLevel LogLevel { set; get; } = LogLevel.Debug;

        public Collection<RoleManagementMessageStore> ReactiveRoles { set; get; } = new Collection<RoleManagementMessageStore>();
        public Collection<SnippetStore> Snippets { set; get; } = new Collection<SnippetStore>();

        [JsonIgnore]
        public bool RunAsUnit { set; get; } = false;

        private static JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public async Task Write(FileInfo file)
        {
            if (!file.Directory.Exists)
                file.Directory.Create();
            using (var fs = file.Create())
                await JsonSerializer.SerializeAsync<Settings>(fs, this, options);
        }

        public static async Task<Settings> Read(FileInfo file)
        {
            using (var fs = file.OpenRead())
                return await JsonSerializer.DeserializeAsync<Settings>(fs);
        }

        public async Task<string> ExportAsync()
        {
            using (var ms = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync<Settings>(ms, this, options);
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