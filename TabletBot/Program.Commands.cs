using System;
using System.Linq;
using System.Reflection;
using TabletBot.Common;
using TabletBot.Common.Attributes.Bot;

#nullable enable

namespace TabletBot
{
    internal partial class Program
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
        public static void ListSettings(params string[] args)
        {
            using (var box = new Box("Settings - " + Settings.File.FullName))
            {
                var output = Settings.ToString();
                box.WriteLine(output);
            }
        }

        [Command]
        public static void ListState(params string[] args)
        {
            using (var box = new Box("State - " + State.File.FullName))
            {
                var output = State.ToString();
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
        public static void SaveSettings(params string[] args)
        {
            try
            {
                if (Settings!.Mutable)
                {
                    Settings.Write();
                    Log.Write("Settings", $"Saved settings to '{Settings.File.FullName}'.");
                }
                else
                {
                    Log.Write("Settings", $"Settings were not saved to '{Settings.File.FullName}' as the file is immutable.", LogLevel.Error);
                }

                State!.Write();;
                Log.Write("Settings", $"Saved state to '{State.File.FullName}'.");
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
    }
}