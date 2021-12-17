using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using TabletBot.Discord.SlashCommands;

namespace TabletBot.Discord.Watchers
{
    public static class Extensions
    {
        public static IServiceCollection AddMessageWatcher<T>(this IServiceCollection serviceCollection)
            where T : class, IMessageWatcher
        {
            return serviceCollection.AddSingleton<IMessageWatcher, T>();
        }

        public static IServiceCollection AddReactionWatcher<T>(this IServiceCollection serviceCollection)
            where T : class, IReactionWatcher
        {
            return serviceCollection.AddSingleton<IReactionWatcher, T>();
        }

        public static IServiceCollection AddCommandModule<T>(this IServiceCollection serviceCollection)
            where T : ModuleBase<ICommandContext>
        {
            return serviceCollection.AddScoped<ModuleBase<ICommandContext>, T>();
        }

        public static IServiceCollection AddSlashCommandModule<T>(this IServiceCollection serviceCollection)
            where T : SlashCommandModule
        {
            return serviceCollection.AddScoped<SlashCommandModule, T>();
        }
    }
}