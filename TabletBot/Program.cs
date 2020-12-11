using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;
using TabletBot.Common;
using TabletBot.Discord;
using TabletBot.GitHub;

namespace TabletBot
{
    partial class Program
    {
        static async Task Main(string[] args)
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
                new Option<bool?>("--unit", "Runs the bot as a unit." )
                {
                    Argument = new Argument<bool?>("unit")
                },
                new Option<LogLevel?>("--level", "Limits logging to a specific minimum log level.")
                {
                    Argument = new Argument<LogLevel?>("level")
                }
            };

            root.Handler = CommandHandler.Create<string, string, bool?, LogLevel?>((discordToken, githubToken, unit, level) =>
            {
                if (discordToken != null)
                    Settings.Current.DiscordBotToken = discordToken;
                if (githubToken != null)
                    Settings.Current.GitHubToken = githubToken;
                if (unit is bool useUnit)
                    Settings.Current.RunAsUnit = useUnit;
                if (level is LogLevel limitLevel)
                    Settings.Current.LogLevel = limitLevel;
            });

            if (await root.InvokeAsync(args) != 0)
                return;

            Log.Output += (message) =>
            {
                if (message.Level >= Settings.Current.LogLevel)
                    IO.WriteLine(message);
            };

            if (!Settings.Current.RunAsUnit)
                IO.WriteMessageHeader();

            await Task.WhenAll(
                Bot.Current.Setup(Settings.Current),
                AuthenticateGitHub(Settings.Current.GitHubToken)
            );

            while (Bot.Current.IsRunning)
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
            await Bot.Current.Logout();
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

        private static async Task AuthenticateGitHub(string token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                try
                {
                    GitHubAPI.Current = new GitHubAPI("TabletBot", token);
                    Settings.Current.GitHubToken = token;
                    await Log.WriteAsync("GitHub", "Authenticated client.");
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
        }
    }
}
