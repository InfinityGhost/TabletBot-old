using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using TabletBot.Common;
using TabletBot.Common.Reflection;
using TabletBot.Discord.Commands;
using TabletBot.Discord.SlashCommands;

namespace TabletBot.Discord
{
    public partial class Bot
    {
        private CommandService CommandService = new CommandService();
        private IServiceProvider Services = new ServiceCollection()
            .BuildServiceProvider();

        public bool CommandsRegistered { private set; get; } = false;
        public bool SlashCommandsRegistered { private set; get; } = false;

        public IReadOnlyCollection<Type> Commands { private set; get; } = new Collection<Type>
        {
            typeof(ModerationCommands),
            typeof(GitHubCommands),
            typeof(RoleCommands),
            typeof(SnippetCommands),
            typeof(UserCommands),
            typeof(HelpCommands)
        };

        public IReadOnlyCollection<Type> SlashCommands { private set; get; } = new Collection<Type>
        {
            typeof(SnippetSlashCommands)
        };

        private IList<SlashCommandModule> SlashCommandModules { get; } = new List<SlashCommandModule>();

        public async Task UpdateSlashCommands()
        {
            foreach (var module in SlashCommandModules)
            {
                try
                {
                    await module.Update(DiscordClient);
                    await Log.WriteAsync("CommandSvc", $"Reregistered slash command module '{module.GetType().Name}'.", LogLevel.Debug);
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }
        }

        private async Task RegisterCommands()
        {
            if (!CommandsRegistered)
            {
                await Log.WriteAsync("CommandSvc", "Registering commands...", LogLevel.Debug);
                foreach (var module in Commands)
                {
                    await CommandService.AddModuleAsync(module, Services);
                    await Log.WriteAsync("CommandSvc", $"Registered command module '{module.Name}'.", LogLevel.Debug);
                }
                CommandService.CommandExecuted += CommandExecuted;
                CommandsRegistered = true;
                await Log.WriteAsync("CommandSvc", "Successfully registered commands.", LogLevel.Debug);
            }
        }

        private async Task RegisterSlashCommands()
        {
            if (!SlashCommandsRegistered)
            {
                await Log.WriteAsync("CommandSvc", "Registering slash commands...", LogLevel.Debug);
                foreach (var module in SlashCommands)
                {
                    try
                    {
                        var instance = module.GetTypeInfo().Construct<SlashCommandModule>();
                        await instance.Hook(DiscordClient);
                        SlashCommandModules.Add(instance);
                        await Log.WriteAsync("CommandSvc", $"Registered slash command module '{module.Name}'.", LogLevel.Debug);
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex);
                    }
                }
                SlashCommandsRegistered = true;
                await Log.WriteAsync("CommandSvc", "Successfully registered slash commands.", LogLevel.Debug);
            }
        }

        private async Task HandleCommand(IMessage message)
        {
            if (CommandsRegistered)
            {
                if (message.Content.StartsWith(Settings.Current.CommandPrefix))
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
                IMessage msg;
                switch (result.Error)
                {
                    case CommandError.BadArgCount:
                        msg = await context.Channel.SendMessageAsync("Error: incorrect argument count.");
                        break;
                    default:
                        msg = await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
                        break;
                }
                DiscordExtensions.DeleteAllDelayed(context.Message, msg);
            }
        }
    }
}