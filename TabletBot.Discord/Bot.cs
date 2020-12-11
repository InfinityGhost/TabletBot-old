using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Octokit;
using TabletBot.Common;
using TabletBot.Discord.Embeds;
using TabletBot.GitHub;

namespace TabletBot.Discord
{
    public partial class Bot
    {
        public Bot()
        {
            DiscordClient.Log += (msg) => LogExtensions.WriteAsync(msg);
            DiscordClient.MessageReceived += MessageReceived;
            DiscordClient.MessageReceived += CheckForIssueRef;
            DiscordClient.Ready += ClientReady;
        }

        public async Task Setup(Settings settings)
        {
            if (settings.DiscordBotToken != null)
                await Login(settings.DiscordBotToken);
            await RegisterCommands();
        }

        public static Bot Current { set; get; } = new Bot();
        public DiscordSocketClient DiscordClient { set; get; } = new DiscordSocketClient();

        public bool IsRunning { set; get; } = true;

        #region Public Methods

        public async Task Login(string token)
        {
            if (token != null)
            {
                await DiscordClient.LoginAsync(TokenType.Bot, token);
                await DiscordClient.StartAsync();
                IsRunning = true;
            }
            else
            {
                throw new NullReferenceException();
            }
        }

        public async Task Logout()
        {
            await DiscordClient.LogoutAsync();
            IsRunning = false;
        }

        public async Task Send(ulong channelId, string message)
        {
            var channel = DiscordClient.GetChannel(channelId);
            if (channel is ITextChannel textChannel)
                await textChannel.SendMessageAsync(message).ConfigureAwait(false);
            else
                throw new InvalidCastException("The channel requested was not a valid text channel.");
        }

        #endregion

        #region Event Handlers

        private async Task ClientReady()
        {
            await Task.WhenAll().ConfigureAwait(false);
        }

        private async Task MessageReceived(IMessage message)
        {
            await Task.WhenAll(
                LogExtensions.WriteAsync(message),
                HandleCommand(message)
            ).ConfigureAwait(false);
        }

        private async Task CheckForIssueRef(IMessage message)
        {
            if (GitHubTools.TryGetIssueRefNumbers(message.Content, out var refs))
            {
                foreach (int issueRef in refs)
                {
                    var issues = await GitHubAPI.Current.Issue.GetAllForRepository("InfinityGhost", "OpenTabletDriver");
                    var prs = await GitHubAPI.Current.PullRequest.GetAllForRepository("InfinityGhost", "OpenTabletDriver");
                    if (issues.FirstOrDefault(i => i.Number == issueRef) is Issue issue)
                    {
                        var pr = prs.FirstOrDefault(pr => pr.Number == issue.Number);
                        var embed = pr == null ? GitHubEmbeds.GetIssueEmbed(issue) : GitHubEmbeds.GetPullRequestEmbed(pr);
                        await message.Channel.SendMessageAsync(embed: embed.Build());
                    }
                }
            }
        }

        #endregion
    }
}
