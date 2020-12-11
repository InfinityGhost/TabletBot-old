using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TabletBot.Common
{
    public static class Platform
    {
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        public static bool IsOSX => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        
        public static DirectoryInfo AppData
        {
            get
            {
                if (IsWindows)
                {
                    var appdata = Environment.GetEnvironmentVariable("LOCALAPPDATA");
                    return new DirectoryInfo(Path.Join(appdata, "TabletBot"));
                }
                else if (IsLinux)
                {
                    var home = Environment.GetEnvironmentVariable("HOME");
                    return new DirectoryInfo(Path.Join(home, ".config", "TabletBot"));
                }
                else if (IsOSX)
                {
                    var macHome = Environment.GetEnvironmentVariable("HOME");
                    return new DirectoryInfo(Path.Join(macHome, "Library", "Application Support", "TabletBot"));
                }
                else
                {
                    return null;
                }
            }
        }

        public static FileInfo SettingsFile => new FileInfo(Path.Join(AppData.FullName, "settings.json"));
    }
}