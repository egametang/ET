using System;
using System.Collections.Generic;

namespace ET
{
    public struct NumericWatcherInfo
    {
        public int SceneType { get; }
        public INumericWatcher INumericWatcher { get; }

        public NumericWatcherInfo(int sceneType, INumericWatcher numericWatcher)
        {
            this.SceneType = sceneType;
            this.INumericWatcher = numericWatcher;
        }
    }

    /// <summary>
    /// 监视数值变化组件,分发监听
    /// </summary>
    [CodeProcess]
    public class NumericWatcherComponent : Singleton<NumericWatcherComponent>, ISingletonAwake
    {
        private readonly Dictionary<int, List<NumericWatcherInfo>> allWatchers = new();
        
        public void Awake()
        {
            HashSet<Type> types = CodeTypes.Instance.GetTypes(typeof(NumericWatcherAttribute));
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(NumericWatcherAttribute), false);

                foreach (object attr in attrs)
                {
                    NumericWatcherAttribute numericWatcherAttribute = (NumericWatcherAttribute)attr;
                    INumericWatcher obj = (INumericWatcher)Activator.CreateInstance(type);
                    NumericWatcherInfo numericWatcherInfo = new(numericWatcherAttribute.SceneType, obj);
                    if (!this.allWatchers.ContainsKey(numericWatcherAttribute.NumericType))
                    {
                        this.allWatchers.Add(numericWatcherAttribute.NumericType, new List<NumericWatcherInfo>());
                    }
                    this.allWatchers[numericWatcherAttribute.NumericType].Add(numericWatcherInfo);
                }
            }
        }
        
        public void Run(Unit unit, NumbericChange args)
        {
            List<NumericWatcherInfo> list;
            if (!this.allWatchers.TryGetValue(args.NumericType, out list))
            {
                return;
            }

            int unitDomainSceneType = unit.IScene.SceneType;
            foreach (NumericWatcherInfo numericWatcher in list)
            {
                if (!SceneTypeSingleton.IsSame(numericWatcher.SceneType, unitDomainSceneType))
                {
                    continue;
                }
                numericWatcher.INumericWatcher.Run(unit, args);
            }
        }
    }
}