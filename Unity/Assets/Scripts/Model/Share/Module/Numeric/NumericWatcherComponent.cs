﻿using System;
using System.Collections.Generic;

namespace ET
{
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
    [Code]
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

            SceneType unitDomainSceneType = unit.IScene.SceneType;
            foreach (NumericWatcherInfo numericWatcher in list)
            {
                if (!numericWatcher.SceneType.HasSameFlag(unitDomainSceneType))
                {
                    continue;
                }
                numericWatcher.INumericWatcher.Run(unit, args);
            }
        }
    }
}