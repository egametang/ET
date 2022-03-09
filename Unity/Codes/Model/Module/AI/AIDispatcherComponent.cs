using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// AI 调查员组件
    /// </summary>
    public class AIDispatcherComponent: Entity, IAwake, IDestroy, ILoad
    {
        public static AIDispatcherComponent Instance;
        
        public Dictionary<string, AAIHandler> AIHandlers = new Dictionary<string, AAIHandler>();
    }
}