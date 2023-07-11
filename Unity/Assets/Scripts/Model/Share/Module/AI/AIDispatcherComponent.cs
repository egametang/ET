using System;
using System.Collections.Generic;

namespace ET
{
    public class AIDispatcherComponent: SingletonLock<AIDispatcherComponent>, ISingletonAwake
    {
        private readonly Dictionary<string, AAIHandler> aiHandlers = new();
        
        public void Awake()
        {
            var types = EventSystem.Instance.GetTypes(typeof (AIHandlerAttribute));
            foreach (Type type in types)
            {
                AAIHandler aaiHandler = Activator.CreateInstance(type) as AAIHandler;
                if (aaiHandler == null)
                {
                    Log.Error($"robot ai is not AAIHandler: {type.Name}");
                    continue;
                }
                this.aiHandlers.Add(type.Name, aaiHandler);
            }
        }

        public AAIHandler Get(string key)
        {
            this.aiHandlers.TryGetValue(key, out var aaiHandler);
            return aaiHandler;
        }

        public override void Load()
        {
            World.Instance.AddSingleton<AIDispatcherComponent>(true);
        }
    }
}