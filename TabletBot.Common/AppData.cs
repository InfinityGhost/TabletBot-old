using System;
using System.IO;

namespace TabletBot.Common
{
    public static class AppData
    {
        private const string SETTINGS_FILENAME = "settings.json";
        private const string STATE_FILENAME = "state.json";

        public static Settings Settings => ImmutableSettings ?? MutableSettings ?? new Settings();
        public static Settings? ImmutableSettings => ImmutableSettingsFile == null ? null : Read<Settings>(ImmutableSettingsFile);
        public static Settings? MutableSettings => Read<Settings>(UserSettingsFile);
        public static State State => Read<State>(StateFile) ?? new State();

        public static FileInfo SettingsFile => ImmutableSettingsFile ?? UserSettingsFile;
        private static FileInfo? ImmutableSettingsFile => string.IsNullOrEmpty(ImmutableSettingsPath) ? null : new FileInfo(ImmutableSettingsPath);
        private static FileInfo UserSettingsFile => new FileInfo(MutableSettingsPath);
        public static FileInfo StateFile => new FileInfo(StatePath);

        private static string? ImmutableSettingsPath => Environment.GetEnvironmentVariable("TABLETBOT_SETTINGS");
        private static string MutableSettingsPath => Path.Join(GetLocalAppdata(), SETTINGS_FILENAME);
        private static string StatePath => Path.Join(GetLocalAppdata(), STATE_FILENAME);

        private static string GetLocalAppdata()
        {
            if (Environment.GetEnvironmentVariable("TABLETBOT_DATA") is string appdata)
            {
                return appdata;
            }
            if (OperatingSystem.IsWindows())
            {
                var localappdata = Environment.GetEnvironmentVariable("LOCALAPPDATA");
                return Path.Join(localappdata, "TabletBot");
            }
            if (OperatingSystem.IsLinux())
            {
                var home = Environment.GetEnvironmentVariable("HOME");
                return Path.Join(home, ".config", "TabletBot");
            }
            if (OperatingSystem.IsMacOS())
            {
                var macHome = Environment.GetEnvironmentVariable("HOME");
                return Path.Join(macHome, "Library", "Application Support", "TabletBot");
            }

            throw new PlatformNotSupportedException("This platform is unsupported.");
        }

        private static T? Read<T>(FileInfo file)
        {
            return file.Exists ? Serialization.Deserialize<T>(file) : default;
        }
    }
}