using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TabletBot.Common;
using TabletBot.Discord.Watchers;

namespace TabletBot.Discord
{
    public partial class Bot
    {
        private bool _messageWatchersRegistered;
        private bool _reactionWatchersRegistered;

        private Task RegisterMessageWatchers(IServiceProvider serviceProvider)
        {
            if (!_messageWatchersRegistered)
            {
                Log.Write("Setup", "Registering message watchers...");
                var watchers = serviceProvider.GetServices<IMessageWatcher>();
                foreach (var watcher in watchers)
                {
                    DiscordClient.MessageReceived += watcher.Receive;
                    Log.Write("Setup", $"Registered message watcher '{watcher.GetType().Name}'.");
                }

                _messageWatchersRegistered = true;
                Log.Write("Setup", "Message watchers successfully registered.");
            }

            return Task.CompletedTask;
        }

        private Task RegisterReactionWatchers(IServiceProvider serviceProvider)
        {
            if (!_reactionWatchersRegistered)
            {
                Log.Write("Setup", "Registering reaction watchers...");
                var watchers = serviceProvider.GetServices<IReactionWatcher>();
                foreach (var watcher in watchers)
                {
                    DiscordClient.ReactionAdded += watcher.ReactionAdded;
                    DiscordClient.ReactionRemoved += watcher.ReactionRemoved;
                    Log.Write("Setup", $"Registered reaction watcher '{watcher.GetType().Name}'.");
                }

                _reactionWatchersRegistered = true;
                Log.Write("Setup", "Reaction watchers successfully registered.");
            }

            return Task.CompletedTask;
        }
    }
}