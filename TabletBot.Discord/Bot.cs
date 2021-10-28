﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Octokit;
using TabletBot.Common;
using TabletBot.Common.Store;
using TabletBot.Discord.Commands;
using TabletBot.Discord.Embeds;
using TabletBot.GitHub;

namespace TabletBot.Discord
{
    using static DiscordExtensions;

    public partial class Bot
    {
        public Bot()
        {
            DiscordClient.Log += (msg) => LogExtensions.WriteAsync(msg);
            DiscordClient.MessageReceived += MessageReceived;
            DiscordClient.MessageReceived += CheckForIssueRef;
            DiscordClient.MessageDeleted += HandleMessageDeleted;
            DiscordClient.Ready += Ready;
        }

        public async Task Setup()
        {
            if (Settings.Current.DiscordBotToken != null)
            {
                await Login(Settings.Current.DiscordBotToken);
            }
        }

        public static Bot Current { set; get; } = new Bot();
        public DiscordSocketClient DiscordClient { set; get; } = new DiscordSocketClient(
            new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true
            }
        );

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

        private async Task Ready()
        {
            DiscordClient.ReactionAdded += HandleReactionAdded;
            DiscordClient.ReactionRemoved += HandleReactionRemoved;
            await Log.WriteAsync("Client", "Hooked reaction events.");

            // Register commands
            await Task.WhenAll(
                RegisterCommands(),
                RegisterSlashCommands()
            );
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
                uint refNum = 0;
                foreach (int issueRef in refs)
                {
                    if (refNum == Settings.Current.GitHubIssueRefLimit)
                        break;

                    var issues = await GitHubAPI.Current.Issue.GetAllForRepository("InfinityGhost", "OpenTabletDriver");
                    var prs = await GitHubAPI.Current.PullRequest.GetAllForRepository("InfinityGhost", "OpenTabletDriver");
                    if (issues.FirstOrDefault(i => i.Number == issueRef) is Issue issue)
                    {
                        var pr = prs.FirstOrDefault(pr => pr.Number == issue.Number);
                        var embed = pr == null ? GitHubEmbeds.GetIssueEmbed(issue) : GitHubEmbeds.GetPullRequestEmbed(pr);
                        await message.Channel.SendMessageAsync(embed: embed);
                    }
                    refNum++;
                }
            }
        }

        private Task HandleMessageDeleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        {
            if (Settings.Current.ReactiveRoles.FirstOrDefault(m => m.MessageId == message.Id) is RoleManagementMessageStore roleStore)
                Settings.Current.ReactiveRoles.Remove(roleStore);

            return Task.CompletedTask;
        }

        private async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            var textChannel = await channel.GetOrDownloadAsync() as ITextChannel;
            await HandleReactionAdded(textChannel, reaction);
        }

        private async Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            var textChannel = await channel.GetOrDownloadAsync() as ITextChannel;
            await HandleReactionRemoved(textChannel, reaction);
        }

        private async Task HandleReactionAdded(ITextChannel channel, SocketReaction reaction)
        {
            try
            {
                if (reaction.IsTracked(out var reactionRole))
                {
                    var guild = await DiscordClient.Rest.GetGuildAsync(channel.GuildId);
                    var role = guild.Roles.FirstOrDefault(r => r.Id == reactionRole.RoleId);
                    var user = await guild.GetUserAsync(reaction.UserId);
                    await user.AddRoleAsync(role);
                }
            }
            catch (Exception ex)
            {
                var systemChannel = await channel.Guild.GetSystemChannelAsync();
                var reply = await ReplyException(systemChannel ?? channel, ex);
                reply.DeleteDelayed();
                Log.Exception(ex);
            }
        }

        private async Task HandleReactionRemoved(ITextChannel channel, SocketReaction reaction)
        {
            try
            {
                if (reaction.IsTracked(out var reactionRole))
                {
                    var guild = await DiscordClient.Rest.GetGuildAsync(channel.GuildId);
                    var role = guild.Roles.FirstOrDefault(r => r.Id == reactionRole.RoleId);
                    var user = await guild.GetUserAsync(reaction.UserId);
                    await user.RemoveRoleAsync(role);
                }
            }
            catch (Exception ex)
            {
                var systemChannel = await channel.Guild.GetSystemChannelAsync();
                var reply = await ReplyException(systemChannel ?? channel, ex);
                reply.DeleteDelayed();
                Log.Exception(ex);
            }
        }

        #endregion
    }
}
