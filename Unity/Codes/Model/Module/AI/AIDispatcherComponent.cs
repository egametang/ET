using System.Collections.Generic;

namespace ET
{
    public class AIDispatcherComponent: Entity
    {
        public static AIDispatcherComponent Instance;
        
        public Dictionary<string, AAIHandler> AIHandlers = new Dictionary<string, AAIHandler>();
    }
}