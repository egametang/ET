using System.Threading;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class NetThreadComponent: Entity, IAwake, ILateUpdate, IDestroy
    {
        [StaticField]
        public static NetThreadComponent Instance;
        
        public Thread thread;
        public bool isStop;
    }
}