using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace TabletBot
{
    public sealed class Settings
    {
        public Settings()
        {
        }

        public static Settings Current { set; get; } = new Settings();

        public string DiscordBotToken { set; get; }
        public string GitHubAPIToken { get; set; }

        public Collection<ulong> SelfRoles { get; set; } = new Collection<ulong>();

        public static readonly XmlSerializer Serializer = new XmlSerializer(typeof(Settings));

        public void Write(FileInfo file)
        {
            if (file.Exists)
                file.Delete();
            using (var fs = file.OpenWrite())
                Serializer.Serialize(fs, this);
        }

        public static Settings Read(FileInfo file)
        {
            using (var fs = file.OpenRead())
                return (Settings)Serializer.Deserialize(fs);
        }
    }
}