using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using TabletBot.Discord.SlashCommands;
using TabletBot.Discord.Watchers;

namespace TabletBot.Discord
{
    public static class Extensions
    {
        public static IServiceCollection AddWatcher<T>(this IServiceCollection serviceCollection)
            where T : class, IWatcher
        {
            return serviceCollection.AddSingleton<IWatcher, T>();
        }

        public static IServiceCollection AddCommandModule<T>(this IServiceCollection serviceCollection)
            where T : ModuleBase<ICommandContext>
        {
            return serviceCollection.AddSingleton<Type>(typeof(T));
        }

        public static IServiceCollection AddSlashCommandModule<T>(this IServiceCollection serviceCollection)
            where T : SlashCommandModule
        {
            return serviceCollection.AddSingleton<SlashCommandModule, T>();
        }

        public static T FirstOfType<TSource, T>(this IEnumerable<TSource> enumerable)
            where T : class, TSource
        {
            return (T)enumerable.First(i => i is T)!;
        }

        public static IEnumerable<Type> OfType<T>(this IEnumerable<Type> types)
        {
            return types.Where(t => t.IsAssignableTo(typeof(T)));
        }
    }
}