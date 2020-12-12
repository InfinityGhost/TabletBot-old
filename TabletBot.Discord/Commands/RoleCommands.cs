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
        public async Task AddReactRole(IRole role, string emote)
        {
            var message = Context.Message.ReferencedMessage;
            await Context.Message.DeleteAsync();
            
            if (message != null)
            {
                try
                {
                    var emoji = new Emoji(emote);
                    await message.AddReactionAsync(emoji);
                }
                catch
                {
                    var e = Emote.Parse(emote);
                    await message.AddReactionAsync(e);
                }
                
                var reactionRole = new RoleManagementMessageStore(message.Id, role.Id, emote);
                Settings.Current.ReactionRoles.Add(reactionRole);
                
                var messageRef = new MessageReference(message.Id, message.Channel.Id);
                var reply = await ReplyAsync($"Reaction role added: {reactionRole.EmoteName}", messageReference: messageRef);
                reply.DeleteDelayed();
            }
            else
            {
                var reply = await ReplyAsync("Error: No message has been referenced.");
                reply.DeleteDelayed();
            }
        }

        [Command("remove-react-role", RunMode = RunMode.Async), Name("Remove reactive role")]
        [RequireUserPermission(GuildPermission.ManageRoles), RequireBotPermission(GuildPermission.ManageRoles | GuildPermission.ManageGuild)]
        public async Task RemoveReactRole(IRole role)
        {
            var message = Context.Message.ReferencedMessage;
            await Context.Message.DeleteAsync();

            var reactionRole = Settings.Current.ReactionRoles.FirstOrDefault(r => r.RoleId == role.Id);
            
            var messageRef = new MessageReference(message.Id, message.Channel.Id);
            var reply = await ReplyAsync($"Reaction role added: {reactionRole.EmoteName}", messageReference: messageRef);
            reply.DeleteDelayed();
        }
    }
}