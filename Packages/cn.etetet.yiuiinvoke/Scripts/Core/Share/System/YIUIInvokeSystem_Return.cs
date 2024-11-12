using System;
using System.Collections.Generic;

namespace ET
{
    public partial class YIUIInvokeSystem
    {
        public R InvokeReturn<T, R>(T self, string invokeType, List<R> returnList = null)
                where T : Entity
        {
            returnList?.Clear();

            try
            {
                var invoker = GetInvoker<IYIUIInvokeReturnHandler<R>>(invokeType);
                if (invoker == null) return default;

                using var list = ListComponent<IYIUIInvokeReturnHandler<R>>.Create();
                YIUIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    var beforeResult = listener.Invoke(self);
                    if (returnList != null)
                    {
                        returnList.Add(beforeResult);
                    }
                }

                var result = invoker.Invoke(self);
                if (returnList != null)
                {
                    returnList.Add(result);
                }

                YIUIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    var afterResult = listener.Invoke(self);
                    if (returnList != null)
                    {
                        returnList.Add(afterResult);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error($"YIUIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name} {e.Message}");
            }

            return default;
        }

        public R InvokeReturn<T, T1, R>(T self, string invokeType, T1 arg1, List<R> returnList = null)
                where T : Entity
        {
            returnList?.Clear();

            try
            {
                var invoker = GetInvoker<IYIUIInvokeReturnHandler<T1, R>>(invokeType);
                if (invoker == null) return default;

                using var list = ListComponent<IYIUIInvokeReturnHandler<T1, R>>.Create();
                YIUIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    var beforeResult = listener.Invoke(self, arg1);
                    if (returnList != null)
                    {
                        returnList.Add(beforeResult);
                    }
                }

                var result = invoker.Invoke(self, arg1);
                if (returnList != null)
                {
                    returnList.Add(result);
                }

                YIUIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    var afterResult = listener.Invoke(self, arg1);
                    if (returnList != null)
                    {
                        returnList.Add(afterResult);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error($"YIUIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name} {e.Message}");
            }

            return default;
        }

        public R InvokeReturn<T, T1, T2, R>(T self, string invokeType, T1 arg1, T2 arg2, List<R> returnList = null)
                where T : Entity
        {
            returnList?.Clear();

            try
            {
                var invoker = GetInvoker<IYIUIInvokeReturnHandler<T1, T2, R>>(invokeType);
                if (invoker == null) return default;

                using var list = ListComponent<IYIUIInvokeReturnHandler<T1, T2, R>>.Create();
                YIUIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    var beforeResult = listener.Invoke(self, arg1, arg2);
                    if (returnList != null)
                    {
                        returnList.Add(beforeResult);
                    }
                }

                var result = invoker.Invoke(self, arg1, arg2);
                if (returnList != null)
                {
                    returnList.Add(result);
                }

                YIUIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    var afterResult = listener.Invoke(self, arg1, arg2);
                    if (returnList != null)
                    {
                        returnList.Add(afterResult);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error($"YIUIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name} {e.Message}");
            }

            return default;
        }

        public R InvokeReturn<T, T1, T2, T3, R>(T self, string invokeType, T1 arg1, T2 arg2, T3 arg3, List<R> returnList = null)
                where T : Entity
        {
            returnList?.Clear();

            try
            {
                var invoker = GetInvoker<IYIUIInvokeReturnHandler<T1, T2, T3, R>>(invokeType);
                if (invoker == null) return default;

                using var list = ListComponent<IYIUIInvokeReturnHandler<T1, T2, T3, R>>.Create();
                YIUIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    var beforeResult = listener.Invoke(self, arg1, arg2, arg3);
                    if (returnList != null)
                    {
                        returnList.Add(beforeResult);
                    }
                }

                var result = invoker.Invoke(self, arg1, arg2, arg3);
                if (returnList != null)
                {
                    returnList.Add(result);
                }

                YIUIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    var afterResult = listener.Invoke(self, arg1, arg2, arg3);
                    if (returnList != null)
                    {
                        returnList.Add(afterResult);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error($"YIUIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name} {e.Message}");
            }

            return default;
        }

        public R InvokeReturn<T, T1, T2, T3, T4, R>(T self, string invokeType, T1 arg1, T2 arg2, T3 arg3, T4 arg4, List<R> returnList = null)
                where T : Entity
        {
            returnList?.Clear();

            try
            {
                var invoker = GetInvoker<IYIUIInvokeReturnHandler<T1, T2, T3, T4, R>>(invokeType);
                if (invoker == null) return default;

                using var list = ListComponent<IYIUIInvokeReturnHandler<T1, T2, T3, T4, R>>.Create();
                YIUIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    var beforeResult = listener.Invoke(self, arg1, arg2, arg3, arg4);
                    if (returnList != null)
                    {
                        returnList.Add(beforeResult);
                    }
                }

                var result = invoker.Invoke(self, arg1, arg2, arg3, arg4);
                if (returnList != null)
                {
                    returnList.Add(result);
                }

                YIUIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    var afterResult = listener.Invoke(self, arg1, arg2, arg3, arg4);
                    if (returnList != null)
                    {
                        returnList.Add(afterResult);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error($"YIUIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name} {e.Message}");
            }

            return default;
        }

        public R InvokeReturn<T, T1, T2, T3, T4, T5, R>(T self, string invokeType, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, List<R> returnList = null)
                where T : Entity
        {
            returnList?.Clear();

            try
            {
                var invoker = GetInvoker<IYIUIInvokeReturnHandler<T1, T2, T3, T4, T5, R>>(invokeType);
                if (invoker == null) return default;

                using var list = ListComponent<IYIUIInvokeReturnHandler<T1, T2, T3, T4, T5, R>>.Create();
                YIUIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    var beforeResult = listener.Invoke(self, arg1, arg2, arg3, arg4, arg5);
                    if (returnList != null)
                    {
                        returnList.Add(beforeResult);
                    }
                }

                var result = invoker.Invoke(self, arg1, arg2, arg3, arg4, arg5);
                if (returnList != null)
                {
                    returnList.Add(result);
                }

                YIUIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    var afterResult = listener.Invoke(self, arg1, arg2, arg3, arg4, arg5);
                    if (returnList != null)
                    {
                        returnList.Add(afterResult);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error($"YIUIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name} {e.Message}");
            }

            return default;
        }

        public async ETTask InvokeTask<T>(T self, string invokeType)
                where T : Entity
        {
            try
            {
                var invoker = GetInvoker<IYIUIInvokeReturnHandler<ETTask>>(invokeType);
                if (invoker == null) return;

                using var list = ListComponent<IYIUIInvokeReturnHandler<ETTask>>.Create();
                YIUIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    await listener.Invoke(self);
                }

                await invoker.Invoke(self);

                YIUIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    await listener.Invoke(self);
                }
            }
            catch (Exception e)
            {
                Log.Error($"YIUIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name} {e.Message}");
            }
        }

        public async ETTask InvokeTask<T, T1>(T self, string invokeType, T1 arg1)
                where T : Entity
        {
            try
            {
                var invoker = GetInvoker<IYIUIInvokeReturnHandler<T1, ETTask>>(invokeType);
                if (invoker == null) return;

                using var list = ListComponent<IYIUIInvokeReturnHandler<T1, ETTask>>.Create();
                YIUIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    await listener.Invoke(self, arg1);
                }

                await invoker.Invoke(self, arg1);

                YIUIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    await listener.Invoke(self, arg1);
                }
            }
            catch (Exception e)
            {
                Log.Error($"YIUIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name} {e.Message}");
            }
        }

        public async ETTask InvokeTask<T, T1, T2>(T self, string invokeType, T1 arg1, T2 arg2)
                where T : Entity
        {
            try
            {
                var invoker = GetInvoker<IYIUIInvokeReturnHandler<T1, T2, ETTask>>(invokeType);
                if (invoker == null) return;

                using var list = ListComponent<IYIUIInvokeReturnHandler<T1, T2, ETTask>>.Create();
                YIUIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    await listener.Invoke(self, arg1, arg2);
                }

                await invoker.Invoke(self, arg1, arg2);

                YIUIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    await listener.Invoke(self, arg1, arg2);
                }
            }
            catch (Exception e)
            {
                Log.Error($"YIUIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name} {e.Message}");
            }
        }

        public async ETTask InvokeTask<T, T1, T2, T3>(T self, string invokeType, T1 arg1, T2 arg2, T3 arg3)
                where T : Entity
        {
            try
            {
                var invoker = GetInvoker<IYIUIInvokeReturnHandler<T1, T2, T3, ETTask>>(invokeType);
                if (invoker == null) return;

                using var list = ListComponent<IYIUIInvokeReturnHandler<T1, T2, T3, ETTask>>.Create();
                YIUIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    await listener.Invoke(self, arg1, arg2, arg3);
                }

                await invoker.Invoke(self, arg1, arg2, arg3);

                YIUIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    await listener.Invoke(self, arg1, arg2, arg3);
                }
            }
            catch (Exception e)
            {
                Log.Error($"YIUIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name} {e.Message}");
            }
        }

        public async ETTask InvokeTask<T, T1, T2, T3, T4>(T self, string invokeType, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
                where T : Entity
        {
            try
            {
                var invoker = GetInvoker<IYIUIInvokeReturnHandler<T1, T2, T3, T4, ETTask>>(invokeType);
                if (invoker == null) return;

                using var list = ListComponent<IYIUIInvokeReturnHandler<T1, T2, T3, T4, ETTask>>.Create();
                YIUIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    await listener.Invoke(self, arg1, arg2, arg3, arg4);
                }

                await invoker.Invoke(self, arg1, arg2, arg3, arg4);

                YIUIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    await listener.Invoke(self, arg1, arg2, arg3, arg4);
                }
            }
            catch (Exception e)
            {
                Log.Error($"YIUIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name} {e.Message}");
            }
        }

        public async ETTask InvokeTask<T, T1, T2, T3, T4, T5>(T self, string invokeType, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
                where T : Entity
        {
            try
            {
                var invoker = GetInvoker<IYIUIInvokeReturnHandler<T1, T2, T3, T4, T5, ETTask>>(invokeType);
                if (invoker == null) return;

                using var list = ListComponent<IYIUIInvokeReturnHandler<T1, T2, T3, T4, T5, ETTask>>.Create();
                YIUIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    await listener.Invoke(self, arg1, arg2, arg3, arg4, arg5);
                }

                await invoker.Invoke(self, arg1, arg2, arg3, arg4, arg5);

                YIUIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    await listener.Invoke(self, arg1, arg2, arg3, arg4, arg5);
                }
            }
            catch (Exception e)
            {
                Log.Error($"YIUIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name} {e.Message}");
            }
        }
    }
}