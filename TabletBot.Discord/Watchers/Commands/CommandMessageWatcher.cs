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
        public CommandMessageWatcher(
            IServiceProvider serviceProvider,
            DiscordSocketClient discordClient,
            CommandService commandService,
            IEnumerable<Type> modules
        )
        {
            _serviceProvider = serviceProvider;
            _discordClient = discordClient;
            _commandService = commandService;
            _commands = modules.OfType<ModuleBase<ICommandContext>>();
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly IDiscordClient _discordClient;
        private readonly CommandService _commandService;
        private readonly IEnumerable<Type> _commands;

        private bool _registered;

        public async Task Receive(IMessage message)
        {
            if (message.Content.StartsWith(Settings.Current.CommandPrefix))
            {
                var context = new CommandContext(_discordClient, message as IUserMessage);
                await _commandService.ExecuteAsync(context, 1, _serviceProvider).ConfigureAwait(false);
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
                await Log.WriteAsync("CommandSvc", $"Registered command module '{module.Name}'.", LogLevel.Debug);
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

                DiscordExtensions.DeleteAllDelayed(context.Message, msg);
            }
        }
    }
}