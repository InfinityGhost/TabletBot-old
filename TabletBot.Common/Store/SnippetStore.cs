namespace TabletBot.Common.Store
{
    public class SnippetStore
    {
        public SnippetStore()
        {
        }

        public SnippetStore(string snippet, string title, string content)
        {
            Snippet = snippet;
            Title = title;
            Content = content;
        }

        /// <summary>
        /// The prefix which will retrieve this snippet.
        /// </summary>
        public string Snippet { set; get; }

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