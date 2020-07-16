using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TabletBot.GitHub.Response;

namespace TabletBot.GitHub
{
    public class WorkflowRun : GitHubObject
    {
        [JsonProperty("head_branch")]
        public string HeadBranch { set; get; }

        [JsonProperty("head_sha")]
        public string HeadSHA { set; get; }

        [JsonProperty("run_number")]
        public uint RunNumber { set; get; }

        [JsonProperty("event")]
        public string Event { set; get; }

        [JsonProperty("status")]
        public string Status { set; get; }

        [JsonProperty("conclusion")]
        public string Conclusion { set; get; }

        [JsonProperty("workflow_id")]
        public uint WorkflowID { set; get; }

        [JsonProperty("url")]
        public string Url { set; get; }

        [JsonProperty("html_url")]
        public string HtmlUrl { set; get; }

        [JsonProperty("pull_requests")]
        public object[] PullRequests { set; get; }

        [JsonProperty("created_at")]
        public DateTime Created { set; get; }

        [JsonProperty("updated_at")]
        public DateTime Updated { set; get; }

        public static async Task<WorkflowRun[]> GetWorkflowRuns(string owner, string repo)
        {
            string url = $"https://api.github.com/repos/{owner}/{repo}/actions/runs";

            using (var httpClient = new HttpClient())
            using (var httpStream = await httpClient.GetStreamAsync(url))
            using (var sr = new StreamReader(httpStream))
            using (var jr = new JsonTextReader(sr))
            {
                var serializer = new JsonSerializer
                {
                    Formatting = Formatting.Indented
                };
                var response = serializer.Deserialize<WorkflowRunResponse>(jr);
                return response.WorkflowRuns;
            }
        }
    }
}