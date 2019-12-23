using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using TabletBot.Common;
using TabletBot.Discord.Commands;

namespace TabletBot.Discord
{
    public partial class Bot
    {
        private CommandService CommandService = new CommandService();
        private IServiceProvider Services = new ServiceCollection()
            .BuildServiceProvider();

        public bool CommandsRegistered { private set; get; } = false;

        public Collection<Type> Commands { private set; get; } = new Collection<Type>()
        {
            typeof(ModerationCommands),
            typeof(GitHubCommands)
        };

        public async Task RegisterCommands()
        {
            if (!CommandsRegistered)
            {
                await Log.WriteAsync("CommandSvc", "Registering commands...");
                foreach (var module in Commands)
                {
                    await CommandService.AddModuleAsync(module, Services);
                    await Log.WriteAsync("CommandSvc", $"Registered module '{module.Name}'.");
                }
                CommandService.CommandExecuted += CommandExecuted;
                CommandsRegistered = true;
                await Log.WriteAsync("CommandSvc", "Successfully registered commands.");
            }
        }

        private async Task HandleCommand(IMessage message)
        {
            if (CommandsRegistered)
            {
                if (message.Content.StartsWith("$"))
                {
                    var context = new CommandContext(DiscordClient, message as IUserMessage);
                    await CommandService.ExecuteAsync(context, 1, Services).ConfigureAwait(false);
                }
            }
        }

        private async Task CommandExecuted(Optional<CommandInfo> cmdInfo, ICommandContext context, IResult result)
        {
            if (result.IsSuccess)
            {
                return;
            }
            else
            {
                switch (result.Error)
                {
                    case CommandError.BadArgCount:
                        await context.Channel.SendMessageAsync("Error: incorrect argument count.");
                        return;
                    default:
                        await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
                        return;
                }
            }
        }
    }
}