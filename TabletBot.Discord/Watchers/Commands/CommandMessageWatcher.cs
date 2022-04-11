using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TabletBot.Common;

namespace TabletBot.Discord.Watchers.Commands
{
    public class CommandMessageWatcher : IMessageWatcher, IAsyncInitialize
    {
        private readonly Settings _settings;
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordSocketClient _discordClient;
        private readonly CommandService _commandService;
        private readonly IEnumerable<Type> _commands;

        public CommandMessageWatcher(
            Settings settings,
            IServiceProvider serviceProvider,
            DiscordSocketClient discordClient,
            CommandService commandService,
            IEnumerable<Type> commands
        )
        {
            _settings = settings;
            _serviceProvider = serviceProvider;
            _discordClient = discordClient;
            _commandService = commandService;
            _commands = commands.OfType<ModuleBase<ICommandContext>>();
        }

        private bool _registered;

        public async Task Receive(IMessage message)
        {
            if (message.Channel is IGuildChannel && !message.Author.IsBot && message.Content.StartsWith(_settings.CommandPrefix))
            {
                try
                {
                    var context = new CommandContext(_discordClient, message as IUserMessage);
                    await _commandService.ExecuteAsync(context, 1, _serviceProvider).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                    try
                    {
                        string exMessage = e.GetType().FullName + ": " + e.Message;
                        await (message as IUserMessage).ReplyAsync(exMessage);
                    }
                    catch (Exception respondEx)
                    {
                        Log.Exception(respondEx);
                    }

                    throw;
                }
            }
        }

        public Task Deleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel) => Task.CompletedTask;

        public async Task InitializeAsync()
        {
            if (_registered)
                return;

            foreach (var module in _commands)
            {
                await _commandService.AddModuleAsync(module, _serviceProvider);
                Log.Write("CommandSvc", $"Registered command module '{module.Name}'.", LogLevel.Debug);
            }

            _commandService.CommandExecuted += CommandExecuted;
            _registered = true;
        }

        private async Task CommandExecuted(Optional<CommandInfo> cmdInfo, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                IMessage msg = result.Error switch
                {
                    CommandError.BadArgCount => await context.Channel.SendMessageAsync("Error: incorrect argument count."),
                    _ => await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}")
                };

                DiscordExtensions.DeleteAllDelayed(5000, context.Message, msg);
            }
        }
    }
}
