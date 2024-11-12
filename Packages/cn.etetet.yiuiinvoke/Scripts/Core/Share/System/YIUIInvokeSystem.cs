using System;
using System.Collections.Generic;
using System.Reflection;

namespace ET
{
    [CodeProcess]
    public partial class YIUIInvokeSystem : Singleton<YIUIInvokeSystem>, ISingletonAwake
    {
        private readonly Dictionary<string, IYIUIInvokeBaseHandler> m_AllInvokers = new();

        public void Awake()
        {
            CodeTypes codeTypes = CodeTypes.Instance;

            foreach (Type type in codeTypes.GetTypes(typeof(YIUIInvokeSystemAttribute)))
            {
                var obj = Activator.CreateInstance(type);
                if (obj is not IYIUIInvokeBaseHandler handler)
                {
                    Log.Error($"类型{type.Name}不是IYIUIInvokeBaseHandler的实现");
                    continue;
                }

                var attribute  = (YIUIInvokeSystemAttribute)type.GetCustomAttribute(typeof(YIUIInvokeSystemAttribute), true);
                var invokeType = attribute.InvokeType;
                handler.InvokeType = invokeType;

                if (!m_AllInvokers.TryAdd(invokeType, handler))
                {
                    Log.Error($"重复添加YIUIInvoke请检查\ntype:{type.Name}\ninvokeType:{invokeType}");
                }
            }
        }

        private T GetInvoker<T>(string invokeType)
        {
            if (m_AllInvokers.TryGetValue(invokeType, out var invoker))
            {
                if (invoker is T tInvoker)
                {
                    return tInvoker;
                }

                Log.Error($"找到YIUIInvoke实现请 但类型不一致请检查 {invokeType}  需求:{typeof(T).Name} 实际:{invoker.GetType().Name}");
                return default;
            }

            Log.Error($"未找到YIUIInvoke实现请检查 {invokeType}  类型:{typeof(T).Name}");
            return default;
        }
    }
}