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
        public Settings()
        {
        }
        
        public const ulong MainGuild = 615607687467761684;

        public static Settings Current { set; get; } = new Settings();

        public ulong GuildID { set; get; } = MainGuild;
        public string DiscordBotToken { set; get; } = null;
        public string GitHubToken { set; get; } = null;
        public string CommandPrefix { set; get; } = "!";
        public LogLevel LogLevel { set; get; } = LogLevel.Debug;

        public Collection<RoleManagementMessageStore> ReactionRoles { set; get; } = new Collection<RoleManagementMessageStore>();

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
    }
}