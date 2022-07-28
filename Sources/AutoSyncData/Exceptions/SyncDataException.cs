using System;
using System.Runtime.Serialization;

namespace AutoSyncData.Exceptions
{
    public class SyncDataException : Exception
    {
        public SyncDataException()
        {
        }

        public SyncDataException(string message)
            : base(message)
        {
        }

        public SyncDataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SyncDataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Description { get; set; }
    }
}
