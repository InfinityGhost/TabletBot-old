using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace TabletBot.Discord.Commands
{
    public class CommandModule : ModuleBase
    {
        public static TimeSpan DeleteDelay { protected set; get; } = TimeSpan.FromSeconds(3);
    }
}