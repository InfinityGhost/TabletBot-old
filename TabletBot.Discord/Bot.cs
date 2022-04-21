using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TabletBot.Common;
using TabletBot.Discord.Watchers;

namespace TabletBot.Discord
{
    public class Bot
    {
        public Bot(
            DiscordSocketClient discordSocketClient,
            IEnumerable<IWatcher> watchers
        )
        {
            _discordSocketClient = discordSocketClient;
            _watchers = watchers;

            _discordSocketClient.Log += LogExtensions.WriteAsync;
            _discordSocketClient.MessageReceived += MessageReceived;
            _discordSocketClient.Ready += Ready;
        }

        private bool _registered;
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly IEnumerable<IWatcher> _watchers;

        public bool IsRunning { set; get; }

        public async Task Login(string token)
        {
            await _discordSocketClient.LoginAsync(TokenType.Bot, token);
            await _discordSocketClient.StartAsync();
            IsRunning = true;
        }

        public async Task Logout()
        {
            await _discordSocketClient.LogoutAsync();
            IsRunning = false;
        }

        public async Task Send(ulong channelId, string message)
        {
            var channel = _discordSocketClient.GetChannel(channelId);
            if (channel is ITextChannel textChannel)
                await textChannel.SendMessageAsync(message).ConfigureAwait(false);
            else
                throw new InvalidCastException("The channel requested was not a valid text channel.");
        }

        private async Task Ready()
        {
            await RegisterWatchers().ConfigureAwait(false);
        }

        private async Task MessageReceived(IMessage message)
        {
            if (message.Author.Id == _discordSocketClient.CurrentUser.Id || !message.Author.IsBot)
                await LogExtensions.WriteAsync(message).ConfigureAwait(false);
        }

        private async Task RegisterWatchers()
        {
            if (_registered)
                return;

            await Task.WhenAll(_watchers.Select(w => RegisterWatcher(w, _discordSocketClient)));

            _registered = true;
        }

        private static async Task RegisterWatcher(IWatcher watcher, DiscordSocketClient discordClient)
        {
            if (watcher is IMessageWatcher messageWatcher)
            {
                discordClient.MessageReceived += messageWatcher.Receive;
                discordClient.MessageDeleted += messageWatcher.Deleted;
            }

            if (watcher is IReactionWatcher reactionWatcher)
            {
                discordClient.ReactionAdded += reactionWatcher.ReactionAdded;
                discordClient.ReactionRemoved += reactionWatcher.ReactionRemoved;
            }

            if (watcher is IInteractionWatcher interactionWatcher)
            {
                discordClient.InteractionCreated += interactionWatcher.HandleInteraction;
            }

            if (watcher is IAsyncInitialize asyncInitialize)
            {
                try
                {
                    await asyncInitialize.InitializeAsync();
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                    Log.Write("Setup", $"Failed to initialize watcher '{watcher.GetType().Name}'.");
                    return;
                }
            }

            Log.Write("Setup", $"Registered watcher '{watcher.GetType().Name}'.");
        }
    }
}
