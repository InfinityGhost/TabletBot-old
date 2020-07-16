using Newtonsoft.Json;

namespace TabletBot.GitHub.Response
{
    public class WorkflowRunResponse
    {
        [JsonProperty("total_count")]
        public uint TotalCount { set; get; }

        [JsonProperty("workflow_runs")]
        public WorkflowRun[] WorkflowRuns { set; get; }
    }
}