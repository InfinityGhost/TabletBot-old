namespace TabletBot
{
    internal class DividerWrap : System.IDisposable
    {
        public DividerWrap()
        {
            Output.WriteDivider();
        }

        public void Dispose()
        {
            Output.WriteDivider();
        }
    }
}