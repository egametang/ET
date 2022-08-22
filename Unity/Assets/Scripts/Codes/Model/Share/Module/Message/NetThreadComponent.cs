using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class NetThreadComponent: Entity, IAwake, ILateUpdate, IDestroy
    {
        [StaticField]
        public static NetThreadComponent Instance;
        
        public const int checkInteral = 2000;
        public const int recvMaxIdleTime = 60000;
        public const int sendMaxIdleTime = 60000;

        public ThreadSynchronizationContext ThreadSynchronizationContext { get; set; }
    }
}