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
        private bool _interactionWatchersRegistered;

        private async Task RegisterMessageWatchers(IServiceProvider serviceProvider)
        {
            if (!_messageWatchersRegistered)
            {
                Log.Write("Setup", "Registering message watchers...");
                var watchers = serviceProvider.GetServices<IMessageWatcher>();
                foreach (var watcher in watchers)
                {
                    try
                    {
                        DiscordClient.MessageReceived += watcher.Receive;
                        if (watcher is IAsyncInitialize asyncInitialize)
                            await asyncInitialize.InitializeAsync();

                        Log.Write("Setup", $"Registered message watcher '{watcher.GetType().Name}'.");
                    }
                    catch (Exception e)
                    {
                        Log.Exception(e);
                        Log.Write("Setup", $"Failed to register message watcher '{watcher.GetType().Name}'.");
                        throw;
                    }
                }

                _messageWatchersRegistered = true;
                Log.Write("Setup", "Message watchers successfully registered.");
            }
        }

        private async Task RegisterReactionWatchers(IServiceProvider serviceProvider)
        {
            if (!_reactionWatchersRegistered)
            {
                Log.Write("Setup", "Registering reaction watchers...");
                var watchers = serviceProvider.GetServices<IReactionWatcher>();
                foreach (var watcher in watchers)
                {
                    try
                    {
                        DiscordClient.ReactionAdded += watcher.ReactionAdded;
                        DiscordClient.ReactionRemoved += watcher.ReactionRemoved;

                        if (watcher is IAsyncInitialize asyncInitialize)
                            await asyncInitialize.InitializeAsync();
                        Log.Write("Setup", $"Registered reaction watcher '{watcher.GetType().Name}'.");
                    }
                    catch (Exception e)
                    {
                        Log.Exception(e);
                        Log.Write("Setup", $"Failed to register reaction watcher '{watcher.GetType().Name}'.");
                        throw;
                    }
                }

                _reactionWatchersRegistered = true;
                Log.Write("Setup", "Reaction watchers successfully registered.");
            }
        }

        private async Task RegisterInteractionWatchers(IServiceProvider serviceProvider)
        {
            if (!_interactionWatchersRegistered)
            {
                Log.Write("Setup", "Registering interaction watchers...");
                var watchers = serviceProvider.GetServices<IInteractionWatcher>();
                foreach (var watcher in watchers)
                {
                    try
                    {
                        DiscordClient.InteractionCreated += watcher.HandleInteraction;

                        if (watcher is IAsyncInitialize asyncInitialize)
                            await asyncInitialize.InitializeAsync();

                        Log.Write("Setup", $"Registered interaction watcher '{watcher.GetType().Name}'.");
                    }
                    catch (Exception e)
                    {
                        Log.Exception(e);
                        Log.Write("Setup", $"Failed to register interaction watcher '{watcher.GetType().Name}'.");
                        throw;
                    }
                }

                _interactionWatchersRegistered = true;
                Log.Write("Setup", "Interaction watchers successfully registered.");
            }
        }
    }
}