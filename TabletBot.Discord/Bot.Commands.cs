using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
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
                foreach (var module in Commands)
                    await CommandService.AddModuleAsync(module, Services);
                CommandService.CommandExecuted += CommandExecuted;
            }
        }

        private async Task HandleCommand(IMessage message)
        {
            if (CommandsRegistered)
            {
                var context = new CommandContext(Client, message as IUserMessage);
                await CommandService.ExecuteAsync(context, 0, Services).ConfigureAwait(false);
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