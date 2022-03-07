using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using TabletBot.Common;
using TabletBot.Discord;

#nullable enable

namespace TabletBot
{
    partial class Program
    {
        public static Bot? Bot { private set; get; }
        public static Settings? Settings { private set; get; }
        public static DiscordSocketClient? DiscordClient { private set; get; }

        private static async Task Main(string[] args)
        {
            string discordToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN")!;
            string githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN")!;
            Settings = Platform.SettingsFile.Exists ? await Settings.Read(Platform.SettingsFile) : new Settings();

            var root = new RootCommand("TabletBot")
            {
                new Option<bool>("--unit", "Runs the bot as a unit." ),
                new Option<LogLevel?>("--level", "Limits logging to a specific minimum log level.")
            };

            root.Handler = CommandHandler.Create<bool, LogLevel?>((unit, level) =>
            {
                Settings.LogLevel = level ?? default;
                Settings.RunAsUnit = unit;
            });

            if (await root.InvokeAsync(args) != 0)
                return;

            Log.Output += message =>
            {
                if (message.Level >= Settings.LogLevel)
                    IO.WriteLogMessage(message);
            };

            if (!Settings.RunAsUnit)
                IO.WriteMessageHeader();

            var config = new DiscordSocketConfig();
            DiscordClient = new DiscordSocketClient(config);
            var gitHubClient = await AuthenticateGitHub(githubToken);

            var serviceCollection = BotServiceCollection.Build(Settings, DiscordClient, gitHubClient);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            Bot = serviceProvider.GetRequiredService<Bot>();
            await Bot.Login(discordToken);

            while (Bot.IsRunning)
            {
                if (Settings.RunAsUnit)
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

        private static async Task<GitHubClient?> AuthenticateGitHub(string token)
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
