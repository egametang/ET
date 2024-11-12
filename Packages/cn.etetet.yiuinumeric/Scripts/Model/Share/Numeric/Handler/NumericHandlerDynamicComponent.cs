using System;
using System.Collections.Generic;

namespace ET
{
    public struct NumericHandlerDynamicInfo
    {
        public int                    SceneType       { get; }
        public INumericHandlerDynamic INumericHandler { get; }

        public int InvakeParentLayerCount { get; } //非0时精准响应

        public NumericHandlerDynamicInfo(int sceneType, INumericHandlerDynamic numericHandler, int invakeParentLayerCount = 0)
        {
            this.SceneType              = sceneType;
            this.INumericHandler        = numericHandler;
            this.InvakeParentLayerCount = invakeParentLayerCount;
        }
    }

    /// <summary>
    /// 监视数值变化组件,分发监听
    /// 这个是动态分发 直接发到对应的监听组件中
    /// </summary>
    [CodeProcess]
    public class NumericHandlerDynamicComponent : Singleton<NumericHandlerDynamicComponent>, ISingletonAwake
    {
        //3K字典
        //K1 = Entity类型 (被调用那个类 动态类)
        //K2 = Entity类型 (数值系统的父级的类)
        //K3 = 数值ID
        //Value = 具体的类
        private readonly Dictionary<Type, Dictionary<Type, Dictionary<int, List<NumericHandlerDynamicInfo>>>> m_AllHandlers = new();

        private readonly Dictionary<Type, Dictionary<int, int>> m_AllTypeCount = new();

        private readonly Dictionary<Type, Type> m_SystemTypeCache = new();

        private Type GetSystemType(Type changeEntityType)
        {
            if (m_SystemTypeCache.TryGetValue(changeEntityType, out var systemType))
            {
                return systemType;
            }

            var openGenericType   = typeof(INumericHandlerDynamic<>);
            var closedGenericType = openGenericType.MakeGenericType(changeEntityType);

            m_SystemTypeCache.Add(changeEntityType, closedGenericType);

            return closedGenericType;
        }

        private Type GetSystemType(Entity changeEntity)
        {
            var changeEntityType = changeEntity.GetType();

            return GetSystemType(changeEntityType);
        }

        public void Awake()
        {
            m_AllHandlers.Clear();
            m_AllTypeCount.Clear();
            m_SystemTypeCache.Clear();
            var type3 = typeof(NumericChange);
            var types = CodeTypes.Instance.GetTypes(typeof(NumericHandlerDynamicAttribute));
            foreach (var type in types)
            {
                Type tType1 = null; //第一个泛型参数 = 监听的那个类 触发方法那个类
                Type tType2 = null; //第二个泛型参数 = 数值的父级那个类
                if (type.BaseType != null && type.BaseType.GenericTypeArguments.Length == 3)
                {
                    tType1 = type.BaseType.GenericTypeArguments[0];
                    tType2 = type.BaseType.GenericTypeArguments[1];
                    var tType3 = type.BaseType.GenericTypeArguments[2];
                    if (tType3 != type3)
                    {
                        Log.Error($"{type.Name} 的继承 NumericHandlerDynamicSystem 的<T3> 必须是 NumericChange");
                        continue;
                    }
                }
                else
                {
                    Log.Error($"没有找到 {type.Name} 的继承 NumericHandlerDynamicSystem 的<T1,T2,T3>");
                    continue;
                }

                object[] attrs = type.GetCustomAttributes(typeof(NumericHandlerDynamicAttribute), false);

                if (attrs.Length >= 2)
                {
                    Log.Error($"{type.Name} 有多个相同特性 只允许有一个 NumericHandlerDynamicAttribute 默认取第一个");
                }

                var numericHandlerAttribute = (NumericHandlerDynamicAttribute)attrs[0];
                var obj                     = (INumericHandlerDynamic)Activator.CreateInstance(type);
                var numericHandlerInfo =
                        new NumericHandlerDynamicInfo(numericHandlerAttribute.SceneType, obj, numericHandlerAttribute.InvakeParentLayerCount);
                var numeriType = numericHandlerAttribute.NumericType;

                if (!m_AllHandlers.ContainsKey(tType1))
                {
                    m_AllHandlers.Add(tType1, new());
                }

                if (!m_AllHandlers[tType1].ContainsKey(tType2))
                {
                    m_AllHandlers[tType1].Add(tType2, new());
                }

                var tTypeDic = m_AllHandlers[tType1][tType2];

                if (!tTypeDic.ContainsKey(numeriType))
                {
                    tTypeDic.Add(numeriType, new List<NumericHandlerDynamicInfo>());
                }

                tTypeDic[numeriType].Add(numericHandlerInfo);
                GetSystemType(tType2);
                AddTypeCount(tType2, numeriType);
            }
        }

        private void AddTypeCount(Type type2, int numericType)
        {
            if (!m_AllTypeCount.ContainsKey(type2))
            {
                m_AllTypeCount.Add(type2, new());
            }

            if (!m_AllTypeCount[type2].ContainsKey(numericType))
            {
                m_AllTypeCount[type2].Add(numericType, 0);
            }

            m_AllTypeCount[type2][numericType]++;
        }

        private int GetTypeCount(Type type2, int numericType)
        {
            if (!m_AllTypeCount.TryGetValue(type2, out var numericDic))
            {
                return 0;
            }

            return numericDic.GetValueOrDefault(numericType, 0);
        }

        private bool HaveType(Type type2, int numericType)
        {
            return GetTypeCount(type2, numericType) > 0;
        }

        public async ETTask Run(NumericChange args)
        {
            //先触发指定的类型
            await Run(args, args._NumericType);

            //后触发监听 数值0的所有监听 0 = 所有数值
            //注意监听所有数据类型的 仅适合做刷新等操作 不适合做存储什么的操作
            //反正注意与监听指定数值类型的不要冲突
            await Run(args, 0);
        }

        private async ETTask Run(NumericChange data, int runNumericType)
        {
            var changeEntity = data._ChangeEntity;
            if (changeEntity == null) return;

            var type2 = changeEntity.GetType();
            if (!HaveType(type2, data._NumericType)) return;
            var systemType = GetSystemType(type2);
            var queue      = changeEntity.Fiber().EntitySystem.GetQueue(systemType);
            if (queue == null) return;
            int count = queue.Count;
            if (count <= 0) return;

            using var hashSet = HashSetComponent<Entity>.Create();
            while (count-- > 0)
            {
                Entity component = queue.Dequeue();
                if (component == null)
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }

                queue.Enqueue(component);
                if (!hashSet.Add(component))
                {
                    continue;
                }

                var tType1 = component.GetType();
                var tType2 = changeEntity.GetType();

                if (!m_AllHandlers.ContainsKey(tType1))
                {
                    continue;
                }

                if (!m_AllHandlers[tType1].ContainsKey(tType2))
                {
                    continue;
                }

                var tTypeDic = m_AllHandlers[tType1][tType2];

                if (!tTypeDic.TryGetValue(runNumericType, out var list))
                {
                    continue;
                }

                using var taskList = ListComponent<ETTask>.Create();

                var domainSceneType = changeEntity.IScene.SceneType;
                foreach (var numericHandler in list)
                {
                    // >0 说明需要精准响应
                    if (numericHandler.InvakeParentLayerCount > 0)
                    {
                        Entity checkEntity = component;
                        for (int i = 0; i < numericHandler.InvakeParentLayerCount; i++)
                        {
                            checkEntity = checkEntity?.Parent;
                        }

                        //监听者 与 响应者相同时 = 精准响应 否则就不需要了
                        if (changeEntity != checkEntity)
                        {
                            continue;
                        }
                    }

                    if (numericHandler.SceneType == 0 ||
                        numericHandler.SceneType == domainSceneType)
                    {
                        taskList.Add(numericHandler.INumericHandler.Run(component, changeEntity, data));
                    }
                }

                try
                {
                    await ETTaskHelper.WaitAll(taskList);
                }
                catch (Exception e)
                {
                    Log.Error($"数值动态事件执行失败: {e.Message}");
                }
            }
        }
    }
}
