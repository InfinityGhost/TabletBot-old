using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TabletBot.Common;
using TabletBot.Common.Store;

namespace TabletBot.Discord.Commands
{
    public class RoleCommands : CommandModule
    {

        [Command("add-react-role", RunMode = RunMode.Async), Name("Add reactive role")]
        [RequireUserPermission(GuildPermission.ManageRoles), RequireBotPermission(GuildPermission.ManageRoles | GuildPermission.ManageGuild)]
        public async Task AddReactiveRole(IRole role, string emote)
        {
            var message = Context.Message.ReferencedMessage;
            var messageRef = new MessageReference(message.Id, message.Channel.Id);
            if (message != null)
            {
                var emoji = emote.GetEmote();
                await message.AddReactionAsync(emoji);
                var reactionRole = new RoleManagementMessageStore(message.Id, role.Id, emote);
                Settings.Current.ReactiveRoles.Add(reactionRole);
                await Settings.Current.Overwrite();

                var reply = await ReplyAsync($"Reactive role added: {reactionRole.EmoteName}", messageReference: messageRef);
                reply.DeleteDelayed();
            }
            else
            {
                var reply = await ReplyAsync("Error: No message has been referenced.", messageReference: messageRef);
                reply.DeleteDelayed();
            }
            await Context.Message.DeleteAsync();
        }

        [Command("remove-react-role", RunMode = RunMode.Async), Name("Remove reactive role")]
        [RequireUserPermission(GuildPermission.ManageRoles), RequireBotPermission(GuildPermission.ManageRoles | GuildPermission.ManageGuild)]
        public async Task RemoveReactiveRole(IRole role)
        {
            var message = Context.Message.ReferencedMessage;
            var messageRef = new MessageReference(message.Id, message.Channel.Id);
            if (Settings.Current.ReactiveRoles.FirstOrDefault(r => r.RoleId == role.Id) is RoleManagementMessageStore reactiveRole)
            {
                Settings.Current.ReactiveRoles.Remove(reactiveRole);
                var emoji = reactiveRole.EmoteName.GetEmote();
                await message.RemoveReactionAsync(emoji, Bot.Current.DiscordClient.CurrentUser);
                await Settings.Current.Overwrite();
                
                var reply = await ReplyAsync($"Reactive role removed from: {reactiveRole.EmoteName}", messageReference: messageRef);
                reply.DeleteDelayed();
            }
            else
            {
                var reply = await ReplyAsync($"{role.Name} is not assigned as reactive to the referenced message.");
                reply.DeleteDelayed();
            }
            await Context.Message.DeleteAsync();
        }
    }
}