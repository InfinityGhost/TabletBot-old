using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace TabletBot.Common
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors | ImplicitUseTargetFlags.WithMembers)]
    public abstract class Serializable
    {
        [JsonIgnore]
        public abstract FileInfo File { get; }

        [JsonIgnore]
        public bool Mutable
        {
            get
            {
                if (File.Exists)
                    return (File.Attributes & FileAttributes.ReadOnly) == 0;

                var dir = File.Directory;
                while (dir is { Exists: false })
                    dir = dir.Parent;

                if (dir == null)
                    return false;

                return (dir.Attributes & FileAttributes.ReadOnly) == 0;
            }
        }

        public void Write()
        {
            if (!Mutable)
            {
                Log.Write("IO", $"Unable to write to '{File.FullName}'.", LogLevel.Error);
                return;
            }

            if (File.Directory is { Exists: false })
                File.Directory.Create();

            using (var fs = File.Create())
                Serialization.Serialize(fs, this);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}