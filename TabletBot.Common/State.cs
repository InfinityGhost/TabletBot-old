using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using TabletBot.Common.Store;

namespace TabletBot.Common
{
    public sealed class State : Serializable
    {
        public Collection<RoleManagementMessage> ReactiveRoles { set; get; } = new Collection<RoleManagementMessage>();
        public Collection<Snippet> Snippets { set; get; } = new Collection<Snippet>();

        [JsonIgnore]
        public bool RunAsUnit { set; get; }

        public override FileInfo File { get; } = AppData.StateFile;
    }
}
