using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TabletBot.GitHub
{
    public class Artifact : GitHubObject
    {
        private Artifact()
        {
        }

        const string API = "https://api.github.com";

        /// <summary>
        /// The name of the artifact.
        /// </summary>
        [JsonProperty("name")]
        public string Name { private set; get; }

        /// <summary>
        /// The size of the artifact in bytes.
        /// </summary>
        [JsonProperty("size_in_bytes")]
        public string Size { private set; get; }

        /// <summary>
        /// The artifact API URL.
        /// </summary>
        [JsonProperty("url")]
        public string Url { private set; get; }

        /// <summary>
        /// The artifact download URL.
        /// </summary>
        [JsonProperty("archive_download_url")]
        public string ArchiveDownloadUrl { private set; get; }

        /// <summary>
        /// Whether the artifact has expired yet. If expired, the download url will be invalid.
        /// </summary>
        [JsonProperty("expired")]
        public bool Expired { private set; get; }

        /// <summary>
        /// When the artifact was created.
        /// </summary>
        [JsonProperty("created_at")]
        public DateTime Created { private set; get; }

        /// <summary>
        /// When the artifact was last updated.
        /// </summary>
        [JsonProperty("updated_at")]
        public DateTime Updated { private set; get; }

        public async Task<string> GetDownloadRedirect(string token)
        {
            using (var handler = new HttpClientHandler { AllowAutoRedirect = false })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue($"token", token);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                client.DefaultRequestHeaders.Add("User-Agent", "TabletBot");

                var response = await client.GetAsync(ArchiveDownloadUrl);
                return Uri.EscapeUriString(Uri.UnescapeDataString(response.Headers.Location.ToString()));
            }
        }

        public static async Task<Artifact[]> GetArtifactsForRun(string owner, string repo, string runUrl)
        {
            var actionRunRegex = new Regex($"github.com/{owner}/{repo}/actions/runs/(?<RunID>.+$)");
            var runRegex = new Regex($"github.com/{owner}/{repo}/runs/?<JobID>");
            if (actionRunRegex.IsMatch(runUrl))
            {
                var match = actionRunRegex.Match(runUrl);
                var runId = match.Groups["RunID"].Value;
                var url = $"{API}/repos/{owner}/{repo}/actions/runs/{runId}/artifacts";
                return await GetArtifacts(url);
            }
            else
            {
                throw new ArgumentException("Invalid run URL was passed.", nameof(runUrl));
            }
        }

        public static async Task<Artifact[]> GetAllArtifacts(string owner, string repo)
        {
            var url = $"{API}/repos/{owner}/{repo}/actions/artifacts";
            return await GetArtifacts(url);
        }

        private static async Task<Artifact[]> GetArtifacts(string url)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                client.DefaultRequestHeaders.Add("User-Agent", "TabletBot");

                using (var httpStream = await client.GetStreamAsync(url))
                using (var sr = new StreamReader(httpStream))
                using (var jr = new JsonTextReader(sr))
                {
                    var serializer = new JsonSerializer
                    {
                        Formatting = Formatting.Indented
                    };
                    var response = serializer.Deserialize<ArtifactResponse>(jr);
                    return response.Artifacts;
                }
            }
        }
    }
}