using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TabletBot.Common;
using TabletBot.Common.Store;
using TabletBot.Discord.Commands.Attributes;

namespace TabletBot.Discord.Commands
{
    [Module]
    public class RoleCommands : CommandModule
    {
        private readonly State _state;
        private readonly DiscordSocketClient _discordSocketClient;

        public RoleCommands(State state, DiscordSocketClient discordSocketClient)
        {
            _state = state;
            _discordSocketClient = discordSocketClient;
        }

        private IList<RoleManagementMessage> ReactiveRoles => _state.ReactiveRoles;

        [Command("add-react-role", RunMode = RunMode.Async), Name("Add reactive role")]
        [RequireUserPermission(GuildPermission.ManageRoles), RequireBotPermission(GuildPermission.ManageRoles | GuildPermission.ManageGuild)]
        public async Task AddReactiveRole(IRole role, string emote)
        {
            var message = Context.Message.ReferencedMessage!;

            var emoji = emote.GetEmote();
            await message.AddReactionAsync(emoji);
            var reactionRole = new RoleManagementMessage(message.Id, role.Id, emote);
            ReactiveRoles.Add(reactionRole);
            _state.Write();

            await ReplyAsync($"Reactive role added: {reactionRole.EmoteName}", messageReference: message.ToReference());
        }

        [Command("remove-react-role", RunMode = RunMode.Async), Name("Remove reactive role")]
        [RequireUserPermission(GuildPermission.ManageRoles), RequireBotPermission(GuildPermission.ManageRoles | GuildPermission.ManageGuild)]
        public async Task RemoveReactiveRole(IRole role)
        {
            var message = Context.Message.ReferencedMessage!;
            var messageRef = new MessageReference(message.Id, message.Channel.Id);
            if (ReactiveRoles.FirstOrDefault(r => r.RoleId == role.Id) is RoleManagementMessage reactiveRole)
            {
                ReactiveRoles.Remove(reactiveRole);
                var emoji = reactiveRole.EmoteName.GetEmote();
                await message.RemoveReactionAsync(emoji, _discordSocketClient.CurrentUser);
                _state.Write();
                
                await ReplyAsync($"Reactive role removed from: {reactiveRole.EmoteName}", messageReference: messageRef);
            }
            else
            {
                await ReplyAsync($"{role.Name} is not assigned as reactive to the referenced message.");
            }
        }
    }
}