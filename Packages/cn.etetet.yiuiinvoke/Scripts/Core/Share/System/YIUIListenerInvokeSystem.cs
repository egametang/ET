using System;
using System.Collections.Generic;
using System.Reflection;

namespace ET
{
    [CodeProcess]
    public partial class YIUIListenerInvokeSystem : Singleton<YIUIListenerInvokeSystem>, ISingletonAwake
    {
        private readonly Dictionary<string, List<(int, IYIUIInvokeBaseHandler)>> m_AllListenerInvokersBefore = new();
        private readonly Dictionary<string, List<(int, IYIUIInvokeBaseHandler)>> m_AllListenerInvokersAfter  = new();

        public void Awake()
        {
            CodeTypes codeTypes = CodeTypes.Instance;
            m_AllListenerInvokersBefore.Clear();
            m_AllListenerInvokersAfter.Clear();

            foreach (Type type in codeTypes.GetTypes(typeof(YIUIListenerInvokeSystemAttribute)))
            {
                var obj = Activator.CreateInstance(type);
                if (obj is not IYIUIInvokeBaseHandler handler)
                {
                    Log.Error($"类型{type.Name}不是IYIUIInvokeBaseHandler的实现");
                    continue;
                }

                var attribute  = (YIUIListenerInvokeSystemAttribute)type.GetCustomAttribute(typeof(YIUIListenerInvokeSystemAttribute), true);
                var invokeType = attribute.InvokeType;
                var priority   = attribute.Priority;
                handler.InvokeType = invokeType;

                if (priority >= 0)
                {
                    if (!m_AllListenerInvokersAfter.ContainsKey(invokeType))
                    {
                        m_AllListenerInvokersAfter.Add(invokeType, new());
                    }

                    var list = m_AllListenerInvokersAfter[invokeType];
                    list.Add((priority, handler));
                }
                else
                {
                    if (!m_AllListenerInvokersBefore.ContainsKey(invokeType))
                    {
                        m_AllListenerInvokersBefore.Add(invokeType, new());
                    }

                    var list = m_AllListenerInvokersBefore[invokeType];
                    list.Add((priority, handler));
                }
            }

            foreach (var key in m_AllListenerInvokersBefore.Keys)
            {
                var list = m_AllListenerInvokersBefore[key];
                SortListenerInvoker(list);
            }

            foreach (var key in m_AllListenerInvokersAfter.Keys)
            {
                var list = m_AllListenerInvokersAfter[key];
                SortListenerInvoker(list);
            }
        }

        private void SortListenerInvoker(List<(int, IYIUIInvokeBaseHandler)> invokers)
        {
            invokers.Sort((a, b) => a.Item1.CompareTo(b.Item1));
        }

        public bool GetListenerInvokerBefore<T>(string invokeType, List<T> list)
        {
            list.Clear();
            GetListenerInvoker(m_AllListenerInvokersBefore, invokeType, list);
            return list.Count > 0;
        }

        public bool GetListenerInvokerAfter<T>(string invokeType, List<T> list)
        {
            list.Clear();
            GetListenerInvoker(m_AllListenerInvokersAfter, invokeType, list);
            return list.Count > 0;
        }

        private void GetListenerInvoker<T>(Dictionary<string, List<(int, IYIUIInvokeBaseHandler)>> all, string invokeType, List<T> list)
        {
            if (all.TryGetValue(invokeType, out var invokerList))
            {
                foreach (var invoke in invokerList)
                {
                    var handler = invoke.Item2;
                    if (handler is T tListenerInvoker)
                    {
                        list.Add(tListenerInvoker);
                    }
                    else
                    {
                        Log.Error($"找到YIUIListenerInvoke实现请 但类型不一致请检查 {invokeType}  需求:{typeof(T).Name} 实际:{invoke.GetType().Name}");
                    }
                }
            }
        }

        public IYIUIInvokeBaseHandler AddListenerInvoker<T>(string invokeType, int priority)
        {
            var tType = typeof(T);
            var obj   = Activator.CreateInstance(tType);
            if (obj is not IYIUIInvokeBaseHandler handler)
            {
                Log.Error($"类型{tType.Name}不是IYIUIInvokeBaseHandler的实现");
                return null;
            }

            handler.InvokeType = invokeType;
            if (priority >= 0)
            {
                if (!m_AllListenerInvokersAfter.ContainsKey(invokeType))
                {
                    m_AllListenerInvokersAfter.Add(invokeType, new());
                }

                var list = m_AllListenerInvokersAfter[invokeType];

                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Item1 > priority)
                    {
                        list.Insert(i, (priority, handler));
                        break;
                    }
                }
            }
            else
            {
                if (!m_AllListenerInvokersBefore.ContainsKey(invokeType))
                {
                    m_AllListenerInvokersBefore.Add(invokeType, new());
                }

                var list = m_AllListenerInvokersBefore[invokeType];
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Item1 > priority)
                    {
                        list.Insert(i, (priority, handler));
                        break;
                    }
                }
            }

            return handler;
        }

        public void RemoveListenerInvoker(IYIUIInvokeBaseHandler handler)
        {
            var invokeType = handler.InvokeType;
            if (m_AllListenerInvokersBefore.TryGetValue(invokeType, out var listBefore))
            {
                for (int i = 0; i < listBefore.Count; i++)
                {
                    if (listBefore[i].Item2 == handler)
                    {
                        listBefore.RemoveAt(i);
                        break;
                    }
                }
            }

            if (m_AllListenerInvokersAfter.TryGetValue(invokeType, out var listAfter))
            {
                for (int i = 0; i < listAfter.Count; i++)
                {
                    if (listAfter[i].Item2 == handler)
                    {
                        listAfter.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }
}