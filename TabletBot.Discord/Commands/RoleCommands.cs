using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TabletBot.Common;

namespace TabletBot.Discord.Commands
{
    public class RoleCommands : CommandModule
    {
        private static bool IsSelfRole(IRole role)
        {
            return Settings.Current.SelfRoles.Contains(role.Id);
        }

        [Command("addrole", RunMode = RunMode.Async), Name("Add Role"), Summary("Adds a self-manageable role to yourself.")]
        public async Task AddRole([Remainder] IRole role)
        {
            await Context.Message.DeleteAsync();
            if (Context.User is IGuildUser guildUser && IsSelfRole(role))
            {
                await guildUser.AddRoleAsync(role);
                var message = await ReplyAsync($"Added the role '{role.Name}'.");
                await message.DeleteDelayed();
            }
            else
            {
                var message = await ReplyAsync($"The role {role.Name} is not a self-manageable role.");
                await message.DeleteDelayed();
            }
        }

        [Command("removerole", RunMode = RunMode.Async), Name("Remove Role"), Summary("Removes a self-manageable role from yourself.")]
        public async Task RemoveRole([Remainder] IRole role)
        {
            await Context.Message.DeleteAsync();
            if (Context.User is IGuildUser guildUser && IsSelfRole(role))
            {
                await guildUser.RemoveRoleAsync(role);
                var message = await ReplyAsync($"Removed the role '{role.Name}'.");
                await message.DeleteDelayed();
            }
            else
            {
                var message = await ReplyAsync($"The role {role.Name} is not a self-manageable role.");
                await message.DeleteDelayed();
            }
        }

        [Command("listroles", RunMode = RunMode.Async), Name("List Roles"), Summary("Lists all self-manageable roles.")]
        public async Task ListRoles()
        {
            await Context.Message.DeleteAsync();
            var message = await ReplyAsync("Fetching self-manageable roles...");
            var selfRoles =
                from role in Context.Guild.Roles
                where Settings.Current.SelfRoles.Contains(role.Id)
                select role.Name;
            
            var embed = new EmbedBuilder
            {
                Title = "Self-Manageable Roles",
                ThumbnailUrl = Bot.Current.DiscordClient.CurrentUser.GetAvatarUrl(),
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Manage with either the `addrole` or the `removerole` command.",
                }
            };
            embed.AddField("Roles", string.Join(", ", selfRoles));
            await message.Update(embed);
        }
    }
}