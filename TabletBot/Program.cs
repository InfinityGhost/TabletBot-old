using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Discord.WebSocket;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using TabletBot.Common;
using TabletBot.Discord;

#nullable enable

namespace TabletBot
{
    internal partial class Program
    {
        private static Settings Settings { get; } = AppData.Settings;
        internal static State State { get; } = AppData.State;
        private static Bot? Bot { set; get; }
        private static DiscordSocketClient? DiscordClient { set; get; }

        [UsedImplicitly]
        private static async Task Main(string[] args)
        {
            string discordToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN")!;
            string githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN")!;

            var root = new RootCommand("TabletBot")
            {
                new Option<bool>("--unit", "Runs the bot as a unit." )
                {
                    Argument = new Argument<bool>("unit")
                },
                new Option<LogLevel?>("--level", "Limits logging to a specific minimum log level.")
                {
                    Argument = new Argument<LogLevel?>("level")
                }
            };

            root.Handler = CommandHandler.Create<bool, LogLevel?>((unit, level) =>
            {
                Settings.LogLevel = level ?? default;
                State.RunAsUnit = unit;
            });

            if (await root.InvokeAsync(args) != 0)
                return;

            Log.Output += message =>
            {
                if (message.Level >= Settings.LogLevel)
                    IO.WriteLogMessage(message);
            };

            if (!State.RunAsUnit)
                IO.WriteMessageHeader();

            var config = new DiscordSocketConfig();
            DiscordClient = new DiscordSocketClient(config);
            var gitHubClient = AuthenticateGitHub(githubToken);

            var serviceCollection = BotServiceCollection.Build(Settings, State, DiscordClient, gitHubClient!);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            Bot = serviceProvider.GetRequiredService<Bot>();
            await Bot.Login(discordToken);

            while (Bot.IsRunning)
            {
                if (State.RunAsUnit)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
                else
                {
                    var commandArgs = IO.ReadLine().Split(' ');
                    InvokeCommand(commandArgs);
                }
            }

            await Bot.Logout();
            Console.WriteLine();
        }

        private static void InvokeCommand(string[] args)
        {
            try
            {
                if (!CommandCollection.Current.Invoke(args))
                    Log.Write("Terminal", $"Invalid command: '{args[0]}'. Execute help for a list of commands.");
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        private static GitHubClient? AuthenticateGitHub(string token)
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

                    Log.Write("GitHub", "Authenticated client.");
                    return githubClient;
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }
            else
            {
                Log.Write("GitHub", "API was not authenticated.", LogLevel.Warning);
            }

            return null;
        }
    }
}
