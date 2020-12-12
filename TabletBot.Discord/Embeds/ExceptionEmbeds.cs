using System;
using Discord;

namespace TabletBot.Discord.Embeds
{
    public static class ExceptionEmbeds
    {
        public static Embed GetEmbedForException(Exception ex)
        {
            var builder = new EmbedBuilder
            {
                Title = ex.GetType().FullName,
                Color = Color.DarkRed,
                Fields =
                {
                    new EmbedFieldBuilder
                    {
                        Name = ex.Message,
                        Value = ex.StackTrace
                    }
                }
            };
            return builder.Build();
        }
    }
}