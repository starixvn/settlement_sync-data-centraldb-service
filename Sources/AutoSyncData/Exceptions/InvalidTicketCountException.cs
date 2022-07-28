using System;
using System.Runtime.Serialization;

namespace AutoSyncData.Exceptions
{
    public class InvalidTicketCountException : SyncDataException
    {
        public InvalidTicketCountException()
        {
            Description = "Failed in validation ticket count";
        }

        public InvalidTicketCountException(string message)
            : base(message)
        {
        }

        public InvalidTicketCountException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected InvalidTicketCountException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
