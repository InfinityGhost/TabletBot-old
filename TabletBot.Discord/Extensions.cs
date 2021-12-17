using System;
using System.Collections.Generic;
using System.Linq;
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

        public static IServiceCollection AddInteractionWatcher<T>(this IServiceCollection serviceCollection)
            where T : class, IInteractionWatcher
        {
            return serviceCollection.AddSingleton<IInteractionWatcher, T>();
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
            return enumerable.First(i => i is T) as T;
        }

        public static IEnumerable<Type> OfType<T>(this IEnumerable<Type> types)
        {
            return types.Where(t => t.IsAssignableFrom(typeof(T)));
        }
    }
}