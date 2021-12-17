using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TabletBot.Common;
using TabletBot.Discord.Watchers;

namespace TabletBot.Discord
{
    public partial class Bot
    {
        private async Task RegisterMessageWatchers(IEnumerable<IMessageWatcher> watchers, DiscordSocketClient discordClient)
        {
            Log.Write("Setup", "Registering message watchers...");
            foreach (var watcher in watchers)
            {
                try
                {
                    discordClient.MessageReceived += watcher.Receive;
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

            Log.Write("Setup", "Message watchers successfully registered.");
        }

        private async Task RegisterReactionWatchers(IEnumerable<IReactionWatcher> watchers, DiscordSocketClient discordClient)
        {
            Log.Write("Setup", "Registering reaction watchers...");
            foreach (var watcher in watchers)
            {
                try
                {
                    discordClient.ReactionAdded += watcher.ReactionAdded;
                    discordClient.ReactionRemoved += watcher.ReactionRemoved;

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

            Log.Write("Setup", "Reaction watchers successfully registered.");
        }

        private async Task RegisterInteractionWatchers(IEnumerable<IInteractionWatcher> watchers, DiscordSocketClient discordClient)
        {
            Log.Write("Setup", "Registering interaction watchers...");
            foreach (var watcher in watchers)
            {
                try
                {
                    discordClient.InteractionCreated += watcher.HandleInteraction;

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

            Log.Write("Setup", "Interaction watchers successfully registered.");
        }
    }
}