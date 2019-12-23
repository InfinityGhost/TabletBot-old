using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace TabletBot.Discord
{
    public partial class Bot
    {
        private Bot()
        {
            Client.Log += (msg) => Log.WriteAsync("Client", msg.Message);
            Client.MessageReceived += MessageReceived;
            Client.Ready += ClientReady;
        }

        public Bot(string token) : this()
        {
            Login(token).GetAwaiter().GetResult();
        }

        public DiscordSocketClient Client { set; get; } = new DiscordSocketClient();

        public bool IsRunning { set; get; } = true;

        #region Public Methods

        public async Task Login(string token)
        {
            if (token != null)
            {
                await Client.LoginAsync(TokenType.Bot, token);
                IsRunning = true;
            }
            else
            {
                throw new NullReferenceException();
            }
        }

        public async Task Logout()
        {
            await Client.LogoutAsync();
            IsRunning = false;
        }

        public async Task Send(ulong channelId, string message)
        {
            var channel = Client.GetChannel(channelId);
            if (channel is ITextChannel textChannel)
                await textChannel.SendMessageAsync(message).ConfigureAwait(false);
            else
                throw new InvalidCastException("The channel requested was not a valid text channel.");
        }

        #endregion

        #region Event Handlers

        private async Task ClientReady()
        {
            await Task.WhenAll(
                Log.WriteAsync("Client", "Ready.")
            ).ConfigureAwait(false);
        }

        private async Task MessageReceived(IMessage message)
        {
            await Task.WhenAll(
                Log.WriteAsync(message),
                HandleCommand(message)
            ).ConfigureAwait(false);
        }

        #endregion
    }
}
