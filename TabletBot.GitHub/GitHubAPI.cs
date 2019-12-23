using Octokit;

namespace TabletBot.GitHub
{
    public class GitHubAPI : GitHubClient
    {
        public GitHubAPI(string productName, string apiKey) : base(new ProductHeaderValue(productName))
        {
            this.Credentials = new Credentials(apiKey);
        }

        public static GitHubAPI Current { set; get; }
    }
}