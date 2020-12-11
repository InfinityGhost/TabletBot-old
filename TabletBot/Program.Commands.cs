using System;
using System.Linq;
using System.Reflection;
using TabletBot.Common;
using TabletBot.Common.Attributes.Bot;
using TabletBot.Discord;

namespace TabletBot
{
    partial class Program
    {
        [Command]
        public static void Help(params string[] args)
        {
            using (var box = new Box("Commands"))
            {
                foreach (var command in CommandCollection.Current)
                {
                    var argsOutput = command.Arguments.Count > 0 ? $"<{string.Join("> <", command.Arguments)}>" : null;
                    var output = $"{command.Method.Name} {argsOutput}";
                    box.WriteLine(output);
                }
            }
        }

        [Command, Alias("Exit")]
        public static void Stop(params string[] args)
        {
            Bot.Current.IsRunning = false;
        }

        [Command("Channel ID", "Message"), Alias("Message")]
        public static async void SendMessage(params string[] args)
        {
            ulong id = Convert.ToUInt64(args[0]);
            string message = string.Join(' ', args[1..^0]);
            await Bot.Current.Send(id, message);
        }

        [Command]
        public static async void ListSettings(params string[] args)
        {
            using (var box = new Box("Settings"))
            {
                var output = await Settings.Current.ExportAsync();
                box.WriteLine(output);
            }
        }

        [Command("Setting", "Value")]
        public static void ModifySetting(params string[] args)
        {
            var setting = args[0];
            var valueStr = string.Concat(args[1..^0]);
            
            if (typeof(Settings).GetProperties().FirstOrDefault(p => p.Name.ToLower() == setting.ToLower()) is PropertyInfo property)
            {
                try
                {
                    object newValue;
                    if (property.PropertyType.IsAssignableTo(typeof(Enum)))
                        newValue = Enum.Parse(property.PropertyType, valueStr);
                    else
                        newValue = Convert.ChangeType(valueStr, property.PropertyType);
                    property.SetValue(Settings.Current, newValue);
                    Log.Write("Settings", "Updated setting.");
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }
            else
            {
                Log.Write("Settings", $"Invalid setting: {setting}", LogLevel.Error);
            }
        }

        [Command]
        public static async void SaveSettings(params string[] args)
        {
            await Settings.Current.Write(Platform.SettingsFile);
            await Log.WriteAsync("Settings", $"Saved to '{Platform.SettingsFile.FullName}'.");
        }

        [Command("Role ID")]
        public static async void AddSelfRole(params string[] args)
        {
            if (Tools.TryGetRole(string.Join(' ', args), out var roleId, out var name))
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

        [Command("Role ID")]
        public static async void RemoveSelfRole(params string[] args)
        {
            if (Tools.TryGetRole(string.Join(' ', args), out var roleId, out var name))
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
    }
}