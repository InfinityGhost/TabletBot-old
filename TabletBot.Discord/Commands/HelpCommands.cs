using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TabletBot.Common;
using TabletBot.Discord.Commands.Attributes;

namespace TabletBot.Discord.Commands
{
    [Module]
    public class HelpCommands : CommandModule
    {
        private readonly Settings _settings;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        private static readonly IList<string> ExcludedModules = new List<string>
        {
            nameof(ModerationCommands)
        };

        public HelpCommands(Settings settings, CommandService commands, IServiceProvider services)
        {
            _settings = settings;
            _commands = commands;
            _services = services;
        }

        [Command("help", RunMode = RunMode.Async), Name("Help"), Summary("Lists all commands available.")]
        public async Task ListCommands()
        {
            var message = await ReplyAsync("Fetching help...");

            IEnumerable<ModuleInfo> modules =
                from module in _commands.Modules
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
                var result = await command.CheckPreconditionsAsync(Context, _services);
                if (result.IsSuccess)
                    command.EmbedParameters(ref embed, _settings);
            }

            await message.Update(embed);
        }
    }
}