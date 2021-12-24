using System;
using System.Linq;
using System.Reflection;
using TabletBot.Common;
using TabletBot.Common.Attributes.Bot;

#nullable enable

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
            Bot!.IsRunning = false;
        }

        [Command("Channel ID", "Message"), Alias("Message")]
        public static async void SendMessage(params string[] args)
        {
            ulong id = Convert.ToUInt64(args[0]);
            string message = string.Join(' ', args[1..^0]);
            await Bot!.Send(id, message);
        }

        [Command]
        public static async void ListSettings(params string[] args)
        {
            using (var box = new Box("Settings"))
            {
                var output = await Settings!.ExportAsync();
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
                    property.SetValue(Settings, newValue);
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
            await Settings!.Write(Platform.SettingsFile);
            await Log.WriteAsync("Settings", $"Saved to '{Platform.SettingsFile.FullName}'.");
        }
    }
}