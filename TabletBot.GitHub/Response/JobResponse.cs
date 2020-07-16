using Newtonsoft.Json;

namespace TabletBot.GitHub.Response
{
    public class JobResponse : GitHubObject
    {
        [JsonProperty("run_id")]
        public int RunID { set; get; }
    }
}