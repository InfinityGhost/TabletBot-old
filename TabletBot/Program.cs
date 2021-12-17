using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using TabletBot.Common;
using TabletBot.Discord;

namespace TabletBot
{
    partial class Program
    {
        public static Bot Bot { private set; get; }
        public static DiscordSocketClient DiscordClient { private set; get; }

        private static async Task Main(string[] args)
        {
            Settings.Current = Platform.SettingsFile.Exists ? await Settings.Read(Platform.SettingsFile) : new Settings();

            var root = new RootCommand("TabletBot")
            {
                new Option<string>(new string[] { "-t", "--discord-token" }, "Sets the bot's Discord API token.")
                {
                    Argument = new Argument<string>("discordToken")
                },
                new Option<string>("--github-token", "Sets the bot's GitHub API token.")
                {
                    Argument = new Argument<string>("githubToken")
                },
                new Option<bool>("--unit", "Runs the bot as a unit." )
                {
                    Argument = new Argument<bool>("unit")
                },
                new Option<LogLevel?>("--level", "Limits logging to a specific minimum log level.")
                {
                    Argument = new Argument<LogLevel?>("level")
                }
            };

            root.Handler = CommandHandler.Create<string, string, bool, LogLevel?>((discordToken, githubToken, unit, level) =>
            {
                Settings.Current.DiscordBotToken ??= discordToken ?? Environment.GetEnvironmentVariable("DISCORD_TOKEN");
                Settings.Current.GitHubToken ??= githubToken ?? Environment.GetEnvironmentVariable("GITHUB_TOKEN");
                
                if (level is LogLevel limitLevel)
                    Settings.Current.LogLevel = limitLevel;

                Settings.Current.RunAsUnit = unit;
            });

            if (await root.InvokeAsync(args) != 0)
                return;

            Log.Output += message =>
            {
                if (message.Level >= Settings.Current.LogLevel)
                    IO.WriteLogMessage(message);
            };

            if (!Settings.Current.RunAsUnit)
                IO.WriteMessageHeader();

            DiscordClient = new DiscordSocketClient(
                new DiscordSocketConfig
                {
                    AlwaysDownloadUsers = true
                }
            );
            var gitHubClient = await AuthenticateGitHub(Settings.Current.GitHubToken);

            var serviceCollection = BotServiceCollection.Build(DiscordClient, gitHubClient);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            Bot = serviceProvider.GetRequiredService<Bot>();

            await Bot.Setup();

            while (Bot.IsRunning)
            {
                if (Settings.Current.RunAsUnit)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
                else
                {
                    var commandArgs = IO.ReadLine().Split(' ');
                    await InvokeCommand(commandArgs);
                }
            }
            await Bot.Logout();
            Console.WriteLine();
        }

        private static async Task InvokeCommand(string[] args)
        {
            try
            {
                if (!CommandCollection.Current.Invoke(args))
                    await Log.WriteAsync("Terminal", $"Invalid command: '{args[0]}'. Execute help for a list of commands.");
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        private static async Task<GitHubClient> AuthenticateGitHub(string token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                try
                {
                    var productHeader = ProductHeaderValue.Parse("TabletBot");
                    var githubClient = new GitHubClient(productHeader)
                    {
                        Credentials = new Credentials(token)
                    };

                    Settings.Current.GitHubToken = token;
                    await Log.WriteAsync("GitHub", "Authenticated client.");
                    return githubClient;
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }
            else
            {
                await Log.WriteAsync("GitHub", "API was not authenticated.", LogLevel.Warning);
            }

            return null;
        }
    }
}
