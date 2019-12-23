using System;
using System.Threading.Tasks;
using TabletBot.Discord;
using TabletBot.Common;
using TabletBot.GitHub;
using System.Linq;

namespace TabletBot
{
    partial class Program
    {
        static BotCommandDictionary BotCommands { set; get; }

        static void InitializeCommands()
        {
            BotCommands = new BotCommandDictionary
            {
                new BotCommand("help", Help),
                new BotCommand("send", SendMessage),
                new BotCommand("github-login", GitHubLogin),
                new BotCommand("list-settings", ListSettings),
                new BotCommand("add-selfrole", AddSelfRole),
                new BotCommand("list-selfroles", ListSelfRoles)
            };
        }

        static async void Help(string args)
        {
            foreach (var cmd in BotCommands)
                await Log.WriteAsync("Help", cmd.Name);
        }

        static async void SendMessage(string args)
        {
            var tokens = args.Split(' ', 2);
            await Bot.Current.Send(Convert.ToUInt64(tokens[0]), tokens[1]);
        }

        static void GitHubLogin(string token)
        {
            GitHubAPI.Current = new GitHubAPI("TabletBot", token);
            Settings.Current.GitHubAPIToken = token;
        }

        static async void ListSettings(string args)
        {
            Console.WriteLine();
            await foreach (var line in Settings.Current.ExportAsync())
                Console.WriteLine(line);
            Console.WriteLine();
        }

        static async void AddSelfRole(string args)
        {
            if (Tools.TryGetRole(args, out var roleId, out var name))
            {
                var role = name != null ? $"'{name}' ({roleId})" : $"{roleId}";

                if (!Settings.Current.SelfRoles.Contains(roleId))
                {
                    Settings.Current.SelfRoles.Add(roleId);
                    await Log.WriteAsync("Settings", $"Added role {role} to self roles.");
                }
                else
                    await Log.WriteAsync("Settings", $"Failed to add role {role} to self roles.");
            }
            else
            {
                await Log.WriteAsync("Settings", $"Role '{args}' does not exist.");
            }
        }

        static async void RemoveSelfRole(string args)
        {
            if (Tools.TryGetRole(args, out var roleId, out var name))
            {
                var role = name != null ? $"'{name}' ({roleId})" : $"{roleId}";
                
                if (Settings.Current.SelfRoles.Remove(roleId))
                    await Log.WriteAsync("Settings", $"Removed role {role} from self roles.");
                else
                    await Log.WriteAsync("Settings", $"Failed to remove role {role} from self roles.");
            }
            else
            {
                await Log.WriteAsync("Settings", $"Role '{args}' does not exist.");
            }
        }

        static async void ListSelfRoles(string args)
        {
            if (Bot.Current != null)
            {
                var selfRoles =
                    from role in Bot.Current.DiscordClient.GetGuild(Settings.Current.GuildID).Roles
                    where Settings.Current.SelfRoles.Contains(role.Id)
                    select role.Name;
                await Log.WriteAsync("SelfRoles", string.Join(", ", selfRoles));
            }
            else
            {
                await Log.WriteAsync("SelfRoles", string.Join(", ", Settings.Current.SelfRoles));
            }
        }
    }
}