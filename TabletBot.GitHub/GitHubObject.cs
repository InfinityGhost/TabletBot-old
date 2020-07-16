using Newtonsoft.Json;

namespace TabletBot.GitHub
{
    public class GitHubObject
    {
        [JsonProperty("id")]
        public uint ID { set; get; }

        [JsonProperty("node_id")]
        public string NodeID { set; get; }
    }
}