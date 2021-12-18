using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using TabletBot.Discord.Commands;
using TabletBot.Discord.SlashCommands;
using TabletBot.Discord.Watchers;
using TabletBot.Discord.Watchers.Commands;
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

        public static IServiceCollection Build(DiscordSocketClient discordClient, GitHubClient gitHubClient)
        {
            return new BotServiceCollection()
                // Core services
                .AddSingleton(discordClient)
                .AddSingleton(gitHubClient)
                .AddSingleton<Bot>()
                .AddSingleton<CommandService>()
                // Message watchers
                .AddMessageWatcher<CommandMessageWatcher>()
                .AddMessageWatcher<IssueMessageWatcher>()
                .AddMessageWatcher<SpamMessageWatcher>()
                // Reaction watchers
                .AddReactionWatcher<RoleReactionWatcher>()
                // Interaction watchers
                .AddInteractionWatcher<SlashCommandInteractionWatcher>()
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