using System;
using System.Collections.Generic;

namespace ET
{
    [FriendOf(typeof(NumericWatcherComponent))]
    public static class NumericWatcherComponentSystem
    {
        [ObjectSystem]
        public class NumericWatcherComponentAwakeSystem : AwakeSystem<NumericWatcherComponent>
        {
            protected override void Awake(NumericWatcherComponent self)
            {
                NumericWatcherComponent.Instance = self;
                self.Init();
            }
        }

	
        public class NumericWatcherComponentLoadSystem : LoadSystem<NumericWatcherComponent>
        {
            protected override void Load(NumericWatcherComponent self)
            {
                self.Init();
            }
        }

        private static void Init(this NumericWatcherComponent self)
        {
            self.allWatchers = new Dictionary<int, List<NumericWatcherInfo>>();

            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof(NumericWatcherAttribute));
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(NumericWatcherAttribute), false);

                foreach (object attr in attrs)
                {
                    NumericWatcherAttribute numericWatcherAttribute = (NumericWatcherAttribute)attr;
                    INumericWatcher obj = (INumericWatcher)Activator.CreateInstance(type);
                    NumericWatcherInfo numericWatcherInfo = new NumericWatcherInfo(numericWatcherAttribute.SceneType, obj);
                    if (!self.allWatchers.ContainsKey(numericWatcherAttribute.NumericType))
                    {
                        self.allWatchers.Add(numericWatcherAttribute.NumericType, new List<NumericWatcherInfo>());
                    }
                    self.allWatchers[numericWatcherAttribute.NumericType].Add(numericWatcherInfo);
                }
            }
        }

        public static void Run(this NumericWatcherComponent self, Unit unit, EventType.NumbericChange args)
        {
            List<NumericWatcherInfo> list;
            if (!self.allWatchers.TryGetValue(args.NumericType, out list))
            {
                return;
            }

            SceneType unitDomainSceneType = unit.DomainScene().SceneType;
            foreach (NumericWatcherInfo numericWatcher in list)
            {
                if (numericWatcher.SceneType != unitDomainSceneType)
                {
                    continue;
                }
                numericWatcher.INumericWatcher.Run(unit, args);
            }
        }
    }

    public class NumericWatcherInfo
    {
        public SceneType SceneType { get; }
        public INumericWatcher INumericWatcher { get; }

        public NumericWatcherInfo(SceneType sceneType, INumericWatcher numericWatcher)
        {
            this.SceneType = sceneType;
            this.INumericWatcher = numericWatcher;
        }
    }
    
    
    /// <summary>
    /// 监视数值变化组件,分发监听
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class NumericWatcherComponent : Entity, IAwake, ILoad
    {
        public static NumericWatcherComponent Instance { get; set; }
		
        public Dictionary<int, List<NumericWatcherInfo>> allWatchers;
    }
}