using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TabletBot.Common;

namespace TabletBot.Discord.Watchers.Commands
{
    public class CommandMessageWatcher : IMessageWatcher
    {
        public CommandMessageWatcher(
            DiscordSocketClient discordClient,
            CommandService commandService,
            IServiceProvider serviceProvider,
            IServiceCollection serviceCollection
        )
        {
            _discordClient = discordClient;
            _commandService = commandService;
            _serviceProvider = serviceProvider;

            _commands = from serviceDescriptor in serviceCollection
                where serviceDescriptor.ServiceType.IsAssignableFrom(typeof(ModuleBase<ICommandContext>))
                select serviceDescriptor.ImplementationType!;
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<Type> _commands;
        private readonly IDiscordClient _discordClient;
        private readonly CommandService _commandService;

        private bool _registered;

        public async Task Receive(IMessage message)
        {
            await RegisterCommands();

            if (message.Content.StartsWith(Settings.Current.CommandPrefix))
            {
                var context = new CommandContext(_discordClient, message as IUserMessage);
                await _commandService.ExecuteAsync(context, 1, _serviceProvider).ConfigureAwait(false);
            }
        }

        public Task Deleted(IMessage message) => Task.CompletedTask;

        private async Task RegisterCommands()
        {
            if (_registered)
                return;

            foreach (var module in _commands)
            {
                await _commandService.AddModuleAsync(module, _serviceProvider);
                await Log.WriteAsync("Setup", $"Registered command module '{module.Name}'.", LogLevel.Debug);
            }

            _commandService.CommandExecuted += CommandExecuted;
            _registered = true;
            await Log.WriteAsync("Setup", "All message commands registered.", LogLevel.Debug);
        }

        private async Task CommandExecuted(Optional<CommandInfo> cmdInfo, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                IMessage msg = result.Error switch
                {
                    CommandError.BadArgCount => await context.Channel.SendMessageAsync(
                        "Error: incorrect argument count."),
                    _ => await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}")
                };

                DiscordExtensions.DeleteAllDelayed(context.Message, msg);
            }
        }
    }
}