using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TabletBot.Common;
using TabletBot.Discord;

namespace TabletBot
{
    partial class Program
    {
        static async Task Main(string[] args)
        {
            Log.Output += (sender, output) =>
            {
                #if !DEBUG
                if (output.Level <= LogLevel.Message)
                    return;
                #endif
                
                var line = string.Format(
                    "{0} {1} {2} | {3}",
                    output.Time.ToLongTimeString(),
                    Output.SetLength($"{output.Level}", 8),
                    Output.SetLength(output.Group, 10),
                    output.Text
                );
                Output.WriteLine(line);
            };
            
            Settings.Current = Platform.SettingsFile.Exists ? Settings.Read(Platform.SettingsFile) : new Settings();

            var root = new RootCommand("TabletBot")
            {
                new Option<string>(new string[] { "-t", "--discord-token" }, "Sets the bot's Discord API token.")
                {
                    Argument = new Argument<string>("discordToken")
                },
                new Option<string>(new string[] { "-g", "--github-token" }, "Sets the bot's GitHub API token.")
                {
                    Argument = new Argument<string>("githubToken")
                },
                new Option<bool>(new string[] { "-u", "--unit" }, "Runs the bot as a unit." )
                {
                    Argument = new Argument<bool>("unit")
                }
            };

            root.Handler = CommandHandler.Create<string, string, bool>((discordToken, githubToken, unit) => 
            {
                if (discordToken != null)
                    Settings.Current.DiscordBotToken = discordToken;
                if (githubToken != null)
                    Settings.Current.GitHubAPIToken = githubToken;
                runAsUnit = unit;
            });

            if (await root.InvokeAsync(args) != 0)
                return;
            
            Bot.Current = new Bot();
            await Bot.Current.Setup(Settings.Current);
            await Task.Run(() => GitHubLogin(Settings.Current.GitHubAPIToken));
            
            while (Bot.Current.IsRunning)
            {
                if (runAsUnit)
                {
                    await Task.Delay(-1);
                }
                else
                {
                    var command = await System.Console.In.ReadLineAsync();
                    await RunCommand(command);
                }
            }
            await Bot.Current.Logout();
        }

        static bool runAsUnit;

        static async Task RunCommand(string args)
        {
            if (!string.IsNullOrWhiteSpace(args))
            {
                try
                {
                    BotCommands.Invoke(args);
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }
            else
            {
                await Log.WriteAsync("CommandError", "Invalid command.", LogLevel.Error);
            }
        }
    }
}
