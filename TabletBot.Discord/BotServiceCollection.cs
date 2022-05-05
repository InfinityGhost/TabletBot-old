using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using TabletBot.Common;
using TabletBot.Discord.Commands;
using TabletBot.Discord.SlashCommands;
using TabletBot.Discord.Watchers.Commands;
using TabletBot.Discord.Watchers.DirectMessage;
using TabletBot.Discord.Watchers.GitHub;
using TabletBot.Discord.Watchers.ReactionRoles;
using TabletBot.Discord.Watchers.Spam;

namespace TabletBot.Discord
{
    public class BotServiceCollection : ServiceCollection
    {
        private BotServiceCollection()
        {
        }

        public static IServiceCollection Build(Settings settings, State state, DiscordSocketClient discordClient, GitHubClient gitHubClient)
        {
            return new BotServiceCollection()
                // Core services
                .AddSingleton(settings)
                .AddSingleton(state)
                .AddSingleton(discordClient)
                .AddSingleton(gitHubClient)
                .AddSingleton<Bot>()
                .AddSingleton<CommandService>()
                // Message watchers
                .AddWatcher<CommandMessageWatcher>()
                .AddWatcher<CodeMessageWatcher>()
                .AddWatcher<IssueMessageWatcher>()
                .AddWatcher<SpamMessageWatcher>()
                .AddWatcher<ModMailMessageWatcher>()
                // Reaction watchers
                .AddWatcher<RoleReactionWatcher>()
                // Interaction watchers
                .AddWatcher<SlashCommandInteractionWatcher>()
                // Commands
                .AddCommandModule<ModerationCommands>()
                .AddCommandModule<GitHubCommands>()
                .AddCommandModule<RoleCommands>()
                .AddCommandModule<SnippetCommands>()
                .AddCommandModule<UserCommands>()
                .AddCommandModule<HelpCommands>()
                // Slash commands
                .AddSlashCommandModule<ModerationSlashCommands>()
                .AddSlashCommandModule<UserSlashCommands>()
                .AddSlashCommandModule<SnippetSlashCommands>();
        }
    }
}