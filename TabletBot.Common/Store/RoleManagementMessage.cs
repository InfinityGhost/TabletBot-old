using JetBrains.Annotations;

namespace TabletBot.Common.Store
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public class RoleManagementMessage
    { 
        public RoleManagementMessage(ulong messageId, ulong targetRole, string emote)
        {
            MessageId = messageId;
            RoleId = targetRole;
            EmoteName = emote;
        }

        public ulong MessageId { set; get; }
        public ulong RoleId { set; get; }
        public string EmoteName { set; get; }
    }
}