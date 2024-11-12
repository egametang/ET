using System;
using System.Collections.Generic;

namespace ET
{
    public struct NumericHandlerInfo
    {
        public int             SceneType       { get; }
        public INumericHandler INumericHandler { get; }

        public NumericHandlerInfo(int sceneType, INumericHandler numericHandler)
        {
            this.SceneType       = sceneType;
            this.INumericHandler = numericHandler;
        }
    }

    /// <summary>
    /// 监视数值变化组件,分发监听
    /// </summary>
    [CodeProcess]
    public class NumericHandlerComponent : Singleton<NumericHandlerComponent>, ISingletonAwake
    {
        //双K   K1 = Entity类型  K2 = 数值ID  Value = 具体的类
        private Dictionary<Type, Dictionary<int, List<NumericHandlerInfo>>> m_AllHandlers;

        public void Awake()
        {
            m_AllHandlers = new();
            var types = CodeTypes.Instance.GetTypes(typeof(NumericHandlerAttribute));
            foreach (var type in types)
            {
                Type tType = null; //当前NumericHandlerSystem<T> 的泛型T
                if (type.BaseType != null && type.BaseType.GenericTypeArguments.Length == 1)
                {
                    tType = type.BaseType.GenericTypeArguments[0];
                }
                else
                {
                    Log.Error($"没有找到 {type.Name} 的继承 NumericHandlerSystem 的<T>");
                    continue;
                }

                object[] attrs = type.GetCustomAttributes(typeof(NumericHandlerAttribute), false);

                if (attrs.Length >= 2)
                {
                    Log.Error($"{type.Name} 有多个相同特性 只允许有一个 NumericHandlerAttribute 默认取第一个");
                }

                var numericHandlerAttribute = (NumericHandlerAttribute)attrs[0];
                var obj                     = (INumericHandler)Activator.CreateInstance(type);
                var numericHandlerInfo      = new NumericHandlerInfo(numericHandlerAttribute.SceneType, obj);
                var numeriType              = numericHandlerAttribute.NumericType;

                if (!m_AllHandlers.ContainsKey(tType))
                {
                    m_AllHandlers.Add(tType, new());
                }

                var tTypeDic = m_AllHandlers[tType];

                if (!tTypeDic.ContainsKey(numeriType))
                {
                    tTypeDic.Add(numeriType, new());
                }

                tTypeDic[numeriType].Add(numericHandlerInfo);
            }
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

        private async ETTask Run(NumericChange args, int runNumericType)
        {
            var changeEntity = args._ChangeEntity;
            if (changeEntity == null) return;
            var changeEntityType = changeEntity.GetType();

            if (!m_AllHandlers.ContainsKey(changeEntityType))
            {
                return;
            }

            var tTypeDic = m_AllHandlers[changeEntityType];

            if (!tTypeDic.TryGetValue(runNumericType, out var list))
            {
                return;
            }

            using ListComponent<ETTask> taskList = ListComponent<ETTask>.Create();

            var domainSceneType = changeEntity.IScene.SceneType;
            foreach (NumericHandlerInfo numericHandler in list)
            {
                if (numericHandler.SceneType == 0 ||
                    numericHandler.SceneType == domainSceneType)
                {
                    taskList.Add(numericHandler.INumericHandler.Run(changeEntity, args));
                }
            }

            try
            {
                await ETTaskHelper.WaitAll(taskList);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}