using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace TabletBot.Common
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public class Serialization
    {
        private static readonly JsonSerializer Serializer = new JsonSerializer
        {
            Formatting = Formatting.Indented
        };

        public static T Deserialize<T>(FileInfo file)
        {
            using (var fs = file.OpenRead())
                return Deserialize<T>(fs);
        }

        public static T Deserialize<T>(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
                return Deserialize<T>(jr);
        }

        public static T Deserialize<T>(JsonTextReader textReader)
        {
            return Serializer.Deserialize<T>(textReader)!;
        }

        public static void Serialize(FileInfo file, object value)
        {
            if (file.Exists)
                file.Delete();

            using (var fs = file.Create())
                Serialize(fs, value);
        }

        public static void Serialize(Stream stream, object value)
        {
            using (var sw = new StreamWriter(stream))
            using (var jw = new JsonTextWriter(sw))
                Serialize(jw, value);
        }

        public static void Serialize(JsonTextWriter textWriter, object value)
        {
            Serializer.Serialize(textWriter, value);
        }
    }
}