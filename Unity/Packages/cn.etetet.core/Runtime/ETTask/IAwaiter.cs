namespace ET
{
    public enum AwaiterStatus: byte
    {
        /// <summary>The operation has not yet completed.</summary>
        Pending = 0,
        
        /// <summary>The operation completed successfully.</summary>
        Succeeded = 1,

        /// <summary>The operation completed with an error.</summary>
        Faulted = 2,
    }
}