using System;
using System.Runtime.Serialization;

namespace AutoSyncData.Exceptions
{
    public class SyncDataFailedException : SyncDataException
    {
        public SyncDataFailedException()
        {
            Description = "Failed to sync data";
        }

        public SyncDataFailedException(string message)
            : base(message)
        {
        }

        public SyncDataFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SyncDataFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
