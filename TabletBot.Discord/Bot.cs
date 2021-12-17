using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TabletBot.Common;
using TabletBot.Common.Store;
using TabletBot.Discord.Watchers;

namespace TabletBot.Discord
{
    public partial class Bot
    {
        public Bot(
            DiscordSocketClient discordSocketClient,
            IEnumerable<IMessageWatcher> messageWatchers,
            IEnumerable<IReactionWatcher> reactionWatchers,
            IEnumerable<IInteractionWatcher> interactionWatchers
        )
        {
            _discordSocketClient = discordSocketClient;
            _messageWatchers = messageWatchers;
            _reactionWatchers = reactionWatchers;
            _interactionWatchers = interactionWatchers;

            _discordSocketClient.Log += LogExtensions.WriteAsync;
            _discordSocketClient.MessageReceived += MessageReceived;
            _discordSocketClient.MessageDeleted += HandleMessageDeleted;
            _discordSocketClient.Ready += Ready;
        }

        private bool _registered;
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly IEnumerable<IMessageWatcher> _messageWatchers;
        private readonly IEnumerable<IReactionWatcher> _reactionWatchers;
        private readonly IEnumerable<IInteractionWatcher> _interactionWatchers;

        public async Task Setup()
        {
            if (Settings.Current.DiscordBotToken != null)
            {
                await Login(Settings.Current.DiscordBotToken);
            }
        }

        public bool IsRunning { set; get; } = true;

        #region Public Methods

        public async Task Login(string token)
        {
            if (token != null)
            {
                await _discordSocketClient.LoginAsync(TokenType.Bot, token);
                await _discordSocketClient.StartAsync();
                IsRunning = true;
            }
            else
            {
                throw new NullReferenceException();
            }
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

        #endregion

        #region Event Handlers

        private async Task Ready()
        {
            if (_registered)
                return;

            await Task.WhenAll(
                RegisterMessageWatchers(_messageWatchers, _discordSocketClient),
                RegisterReactionWatchers(_reactionWatchers, _discordSocketClient),
                RegisterInteractionWatchers(_interactionWatchers, _discordSocketClient)
            );

            _registered = true;
        }

        private async Task MessageReceived(IMessage message)
        {
            await LogExtensions.WriteAsync(message).ConfigureAwait(false);
        }

        private Task HandleMessageDeleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        {
            if (Settings.Current.ReactiveRoles.FirstOrDefault(m => m.MessageId == message.Id) is RoleManagementMessageStore roleStore)
                Settings.Current.ReactiveRoles.Remove(roleStore);

            return Task.CompletedTask;
        }

        #endregion
    }
}
