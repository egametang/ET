using System;

namespace ET
{
    [FriendOf(typeof(AIDispatcherComponent))]
    public static class AIDispatcherComponentSystem
    {
        [ObjectSystem]
        public class AIDispatcherComponentAwakeSystem: AwakeSystem<AIDispatcherComponent>
        {
            protected override void Awake(AIDispatcherComponent self)
            {
                AIDispatcherComponent.Instance = self;
                self.Load();
            }
        }

        [ObjectSystem]
        public class AIDispatcherComponentLoadSystem: LoadSystem<AIDispatcherComponent>
        {
            protected override void Load(AIDispatcherComponent self)
            {
                self.Load();
            }
        }

        [ObjectSystem]
        public class AIDispatcherComponentDestroySystem: DestroySystem<AIDispatcherComponent>
        {
            protected override void Destroy(AIDispatcherComponent self)
            {
                self.AIHandlers.Clear();
                AIDispatcherComponent.Instance = null;
            }
        }
        
        private static void Load(this AIDispatcherComponent self)
        {
            self.AIHandlers.Clear();
            
            var types = EventSystem.Instance.GetTypes(typeof (AIHandlerAttribute));
            foreach (Type type in types)
            {
                AAIHandler aaiHandler = Activator.CreateInstance(type) as AAIHandler;
                if (aaiHandler == null)
                {
                    Log.Error($"robot ai is not AAIHandler: {type.Name}");
                    continue;
                }
                self.AIHandlers.Add(type.Name, aaiHandler);
            }
        }
    }
}