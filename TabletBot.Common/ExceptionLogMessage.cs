using System;

namespace TabletBot.Common
{
    public class ExceptionLogMessage : LogMessage
    {
        public ExceptionLogMessage(Exception ex)
            : base("Exception", ex.Message, LogLevel.Error)
        {
            Exception = ex;
        }

        public Exception Exception { get; }
    }
}