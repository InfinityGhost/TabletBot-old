using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace TabletBot.Discord.Commands
{
    public class HelpCommands : CommandModule
    {
        private readonly CommandService Commands;
        private readonly IServiceProvider Services;

        private static readonly IList<string> ExcludedModules = new List<string>
        {
            nameof(ModerationCommands)
        };

        public HelpCommands(CommandService commands, IServiceProvider services)
        {
            Commands = commands;
            Services = services;
        }

        [Command("help", RunMode = RunMode.Async), Name("Help"), Summary("Lists all commands available.")]
        public async Task ListCommands()
        {
            await Context.Message.DeleteAsync();
            var message = await ReplyAsync("Fetching help...");
            
            IEnumerable<ModuleInfo> modules = 
                from module in Commands.Modules
                where !ExcludedModules.Contains(module.Name)
                where module.Parent == null
                select module;
            
            IEnumerable<CommandInfo> commands = 
                from module in modules
                from command in module.Commands
                select command;

            IUser user = Context.Client.CurrentUser;

            var embed = new EmbedBuilder
            {
                Title = "Command List",
                Color = Color.Orange,
                Author = new EmbedAuthorBuilder
                {
                    Name = user.Username,
                    IconUrl = user.GetAvatarUrl()
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = string.Format("{0} commands shown.", commands.Count()),
                    IconUrl = Context.Client.CurrentUser.GetAvatarUrl()
                }
            };

            foreach (var command in commands)
            {
                var result = await command.CheckPreconditionsAsync(Context, Services);
                if (result.IsSuccess)
                    command.EmbedParameters(ref embed);
            }

            await message.Update(embed);
        }
    }
}