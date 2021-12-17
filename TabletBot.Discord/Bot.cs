using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TabletBot.Common;
using TabletBot.Common.Store;
using TabletBot.Discord.Watchers;

namespace TabletBot.Discord
{
    public partial class Bot
    {
        public Bot()
        {
            DiscordClient.Log += (msg) => LogExtensions.WriteAsync(msg);
            DiscordClient.MessageReceived += MessageReceived;
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
            var serviceCollection = BotServiceCollection.Build(DiscordClient);
            serviceCollection  = serviceCollection.AddSingleton(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            await Task.WhenAll(
                RegisterMessageWatchers(serviceProvider),
                RegisterReactionWatchers(serviceProvider),
                RegisterSlashCommands(serviceProvider)
            );
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
