using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

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
        public string DiscordBotToken { set; get; }
        public string GitHubAPIToken { set; get; }
        public string CommandPrefix { set; get; } = "!";

        [XmlArray("SelfRoles"), XmlArrayItem("Role")]
        public Collection<ulong> SelfRoles { set; get; } = new Collection<ulong>();

        public static readonly XmlSerializer Serializer = new XmlSerializer(typeof(Settings));

        public void Write(FileInfo file)
        {
            if (file.Exists)
                file.Delete();
            if (!file.Directory.Exists)
                file.Directory.Create();
            using (var fs = file.OpenWrite())
                Serializer.Serialize(fs, this);
        }

        public static Settings Read(FileInfo file)
        {
            using (var fs = file.OpenRead())
                return (Settings)Serializer.Deserialize(fs);
        }

        public async IAsyncEnumerable<string> ExportAsync()
        {
            using (var ds = new MemoryStream())
            using (var sr = new StreamReader(ds))
            {
                Serializer.Serialize(ds, this);
                ds.Position = 0;
                while (!sr.EndOfStream)
                    yield return await sr.ReadLineAsync();
            }
        }
    }
}