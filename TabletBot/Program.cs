using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using TabletBot.Common;
using TabletBot.Discord;

namespace TabletBot
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Log.Output += (sender, output) => Console.WriteLine(string.Format("{0}   {1}\t| {2}", output.Time.ToLongTimeString(), output.Group, output.Text));
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            var root = new RootCommand("TabletBot")
            {
                new Option<string>(new string[] { "-t", "--discord-token" }, "Sets the bot's Discord API token.")
                {
                    Argument = new Argument<string>("discordToken")
                },
                new Option<string>("--github-token", "Sets the bot's GitHub API token.")
                {
                    Argument = new Argument<string>("githubToken")
                }
            };

            root.Handler = CommandHandler.Create<string, string>((discordToken, githubToken) => 
            {
                if (discordToken != null)
                    Settings.Current.DiscordBotToken = discordToken;
                if (githubToken != null)
                    Settings.Current.GitHubAPIToken = githubToken;
            });

            await root.InvokeAsync(args);
            
            Bot.Current = new Bot();
            await Bot.Current.Setup(Settings.Current);
            await Task.Run(InitializeCommands);
            
            while (Bot.Current.IsRunning)
            {
                await RunCommand(Console.ReadLine());
            }
        }

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
                await Log.WriteAsync("CommandError", "Invalid command.");
            }
        }
    }
}
