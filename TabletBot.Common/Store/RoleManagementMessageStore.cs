namespace TabletBot.Common.Store
{
    public class RoleManagementMessageStore
    { 
        public RoleManagementMessageStore()
        {
        }

        public RoleManagementMessageStore(ulong messageId, ulong targetRole, string emote)
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