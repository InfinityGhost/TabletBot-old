using Newtonsoft.Json;

namespace TabletBot.GitHub
{
    internal class ArtifactResponse
    {
        [JsonProperty("total_count")]
        public int TotalCount { set; get; }
        
        [JsonProperty("artifacts")]
        public Artifact[] Artifacts { set; get; }
    }
}