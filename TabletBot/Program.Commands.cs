using System;
using System.Threading.Tasks;
using TabletBot.Discord;
using TabletBot.Common;
using TabletBot.GitHub;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace TabletBot
{
    partial class Program
    {
        static BotCommandDictionary BotCommands { set; get; } = new BotCommandDictionary
        {
            new BotCommand("help", Help),
            new BotCommand("stop", Stop),
            new BotCommand("send", SendMessage),
            new BotCommand("github-login", GitHubLogin),
            new BotCommand("save-settings", SaveSettings),
            new BotCommand("add-selfrole", AddSelfRole),
            new BotCommand("remove-selfrole", RemoveSelfRole),
            new BotCommand("list-settings", ListSettings),
            new BotCommand("list-selfroles", ListSelfRoles)
        };

        static void Help(string args)
        {
            var names =
                from command in BotCommands
                select command.Name;
            using (new DividerWrap())
            {
                foreach (var name in names)
                    Output.WriteLine(name);
            }
        }

        static void Stop(string args)
        {
            Bot.Current.IsRunning = false;
        }

        static async void SendMessage(string args)
        {
            var tokens = args.Split(' ', 2);
            await Bot.Current.Send(Convert.ToUInt64(tokens[0]), tokens[1]);
        }

        static async void GitHubLogin(string token)
        {
            GitHubAPI.Current = new GitHubAPI("TabletBot", token);
            Settings.Current.GitHubAPIToken = token;
            await Log.WriteAsync("GitHub", "Authenticated client.");
        }

        static async void ListSettings(string args)
        {
            using (new DividerWrap())
            {
                await foreach (var line in Settings.Current.ExportAsync())
                Output.WriteLine(line);
            }
        }

        static async void SaveSettings(string args)
        {
            Settings.Current.Write(Platform.SettingsFile);
            await Log.WriteAsync("Settings", $"Saved to '{Platform.SettingsFile.FullName}'.");
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

        static void ListSelfRoles(string args)
        {
            if (Bot.Current != null)
            {
                var selfRoles =
                    from role in Bot.Current.DiscordClient.GetGuild(Settings.Current.GuildID).Roles
                    where Settings.Current.SelfRoles.Contains(role.Id)
                    select (role.Name, role.Id);
                using (new DividerWrap())
                {
                    foreach (var role in selfRoles)
                        Output.WriteLine($"{role.Name} ({role.Id})");
                }
            }
            else
            {
                var selfRoles =
                    from role in Settings.Current.SelfRoles
                    select role.ToString();
                    
                using (new DividerWrap())
                {
                    foreach (var role in selfRoles)
                        Output.WriteLine(role);
                }
            }
        }
    }
}