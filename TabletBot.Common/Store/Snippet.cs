using JetBrains.Annotations;

namespace TabletBot.Common.Store
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public class Snippet
    {
        public Snippet(string id, string title, string content)
        {
            ID = id;
            Title = title;
            Content = content;
        }

        /// <summary>
        /// The prefix which will retrieve this snippet.
        /// </summary>
        public string ID { set; get; }

        /// <summary>
        /// The title of the snippet.
        /// </summary>
        public string Title { set; get; }

        /// <summary>
        /// The message in which will be sent in the snippet.
        /// </summary>
        public string Content { set; get; }
    }
}