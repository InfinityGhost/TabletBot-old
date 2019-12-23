using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.CommandLine;
using System.CommandLine.DragonFruit;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.Diagnostics;
using System.IO;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using TabletBot.Discord;
using TabletBot.Discord.Common;

namespace TabletBot
{
    class Program
    {
        public static Bot DiscordBot { set; get; }
        
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            var root = new RootCommand("TabletBot")
            {
                new Option<string>(new string[] { "-t", "--discord-token" }, "Sets the discord bot's token.")
                {
                    Argument = new Argument<string>("discordToken")
                }
            };
            root.Handler = CommandHandler.Create<string>((discordToken) => 
            {
                if (discordToken != null)
                    Settings.Current.DiscordBotToken = discordToken;
                
            });
            
            DiscordBot = new Bot(Settings.Current.DiscordBotToken);

            while (DiscordBot.IsRunning)
            {
            }
        }

        static async Task RunCommand(InvocationContext context, string args)
        {
            var root = new Command("Bot Controls");
            var sendMessage = new Command("send");
            sendMessage.Handler = CommandHandler.Create<ulong, string>(DiscordBot.Send);
            root.AddCommand(sendMessage);
            await root.InvokeAsync(args, context.Console);
        }
    }
}
