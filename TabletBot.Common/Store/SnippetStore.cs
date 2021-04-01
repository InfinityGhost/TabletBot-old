namespace TabletBot.Common.Store
{
    public class SnippetStore
    {
        public SnippetStore()
        {
        }

        public SnippetStore(string prefix, string title, string snippet)
        {
            Prefix = prefix;
            Title = title;
            Content = snippet;
        }

        /// <summary>
        /// The prefix which will retrieve this snippet.
        /// </summary>
        public string Prefix { set; get; }

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