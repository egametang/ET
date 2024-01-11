using System;
using System.Collections.Generic;
using ET;

namespace YIUIFramework
{
    public static class UITaskEventBaseHelper
    {
        #region 创建

        public static UIEventBase CreatorUITaskEventBase(string name, List<EUIEventParamType> paramList)
        {
            UIEventBase uiTaskEventBase = null;
            switch (paramList.Count)
            {
                case 0:
                    uiTaskEventBase = CreatorUITaskEventP0();
                    break;
                case 1:
                    uiTaskEventBase = CreatorUITaskEventP1(paramList);
                    break;
                case 2:
                    uiTaskEventBase = CreatorUITaskEventP2(paramList);
                    break;
                case 3:
                    uiTaskEventBase = CreatorUITaskEventP3(paramList);
                    break;
                case 4:
                    uiTaskEventBase = CreatorUITaskEventP4(paramList);
                    break;
                case 5:
                    uiTaskEventBase = CreatorUITaskEventP5(paramList);
                    break;
                default:
                    Logger.LogError($"最高支持5个参数 如果还想支持就自己扩展 {paramList.Count}");
                    break;
            }

            if (uiTaskEventBase == null)
            {
                Logger.LogError($"{name} 动态创建泛型事件 失败 请检查原因");
                return null;
            }

            uiTaskEventBase.SetName(name);
            uiTaskEventBase.RefreshAllEventParamType(paramList);
            return uiTaskEventBase;
        }

        private static UIEventBase CreatorUITaskEventP0()
        {
            return (UIEventBase)(Activator.CreateInstance(typeof(UITaskEventP0)));
        }

        private static UIEventBase CreatorUITaskEventP1(List<EUIEventParamType> paramList)
        {
            return (UIEventBase)(Activator.CreateInstance(typeof(UITaskEventP1<>)
                .MakeGenericType(
                    paramList[0].GetParamType()
                )));
        }

        private static UIEventBase CreatorUITaskEventP2(List<EUIEventParamType> paramList)
        {
            return (UIEventBase)(Activator.CreateInstance(typeof(UITaskEventP2<,>)
                .MakeGenericType(
                    paramList[0].GetParamType(),
                    paramList[1].GetParamType()
                )));
        }

        private static UIEventBase CreatorUITaskEventP3(List<EUIEventParamType> paramList)
        {
            return (UIEventBase)(Activator.CreateInstance(typeof(UITaskEventP3<,,>)
                .MakeGenericType(
                    paramList[0].GetParamType(),
                    paramList[1].GetParamType(),
                    paramList[2].GetParamType()
                )));
        }

        private static UIEventBase CreatorUITaskEventP4(List<EUIEventParamType> paramList)
        {
            return (UIEventBase)(Activator.CreateInstance(typeof(UITaskEventP4<,,,>)
                .MakeGenericType(
                    paramList[0].GetParamType(),
                    paramList[1].GetParamType(),
                    paramList[2].GetParamType(),
                    paramList[3].GetParamType()
                )));
        }

        private static UIEventBase CreatorUITaskEventP5(List<EUIEventParamType> paramList)
        {
            return (UIEventBase)(Activator.CreateInstance(typeof(UITaskEventP5<,,,,>)
                .MakeGenericType(
                    paramList[0].GetParamType(),
                    paramList[1].GetParamType(),
                    paramList[2].GetParamType(),
                    paramList[3].GetParamType(),
                    paramList[4].GetParamType()
                )));
        }

        #endregion

        #region 调用

        public static async ETTask InvokeAsync(this UIEventBase self)
        {
            if (self is IUITaskEventInvoke uiTaskEventInvoke)
            {
                await uiTaskEventInvoke.Invoke();
            }
            else
            {
                Logger.LogError($"调用 当前事件 {self.EventName} 参数不一致 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }
        }

        public static async ETTask InvokeAsync<P1>(this UIEventBase self, P1 p1)
        {
            if (self is IUITaskEventInvoke<P1> uiTaskEventInvoke)
            {
                await uiTaskEventInvoke.Invoke(p1);
            }
            else
            {
                Logger.LogError(
                    $"调用 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }
        }

        public static async ETTask InvokeAsync<P1, P2>(this UIEventBase self, P1 p1, P2 p2)
        {
            if (self is IUITaskEventInvoke<P1, P2> uiTaskEventInvoke)
            {
                await uiTaskEventInvoke.Invoke(p1, p2);
            }
            else
            {
                Logger.LogError(
                    $"调用 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }
        }

        public static async ETTask InvokeAsync<P1, P2, P3>(this UIEventBase self, P1 p1, P2 p2, P3 p3)
        {
            if (self is IUITaskEventInvoke<P1, P2, P3> uiTaskEventInvoke)
            {
                await uiTaskEventInvoke.Invoke(p1, p2, p3);
            }
            else
            {
                Logger.LogError(
                    $"调用 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }
        }

        public static async ETTask InvokeAsync<P1, P2, P3, P4>(this UIEventBase self, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            if (self is IUITaskEventInvoke<P1, P2, P3, P4> uiTaskEventInvoke)
            {
                await uiTaskEventInvoke.Invoke(p1, p2, p3, p4);
            }
            else
            {
                Logger.LogError(
                    $"调用 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} {typeof(P4).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }
        }

        public static async ETTask InvokeAsync<P1, P2, P3, P4, P5>(this UIEventBase self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            if (self is IUITaskEventInvoke<P1, P2, P3, P4, P5> uiTaskEventInvoke)
            {
                await uiTaskEventInvoke.Invoke(p1, p2, p3, p4, p5);
            }
            else
            {
                Logger.LogError(
                    $"调用 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} {typeof(P4).Name} {typeof(P5).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }
        }

        #endregion

        #region 添加事件

        public static UITaskEventHandleP0 AddTaskEvent(this UIEventBase self, UITaskEventDelegate callback)
        {
            if (self is UITaskEventP0 uiTaskEventParam)
            {
                return uiTaskEventParam.Add(callback);
            }
            else
            {
                Logger.LogError($"添加 当前事件 {self.EventName} 参数不一致 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return null;
        }

        public static UITaskEventHandleP1<P1> AddTaskEvent<P1>(this UIEventBase self, UITaskEventDelegate<P1> callback)
        {
            if (self is UITaskEventP1<P1> uiTaskEventParam)
            {
                return uiTaskEventParam.Add(callback);
            }
            else
            {
                Logger.LogError(
                    $"添加 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return null;
        }

        public static UITaskEventHandleP2<P1, P2> AddTaskEvent<P1, P2>(this UIEventBase self, UITaskEventDelegate<P1, P2> callback)
        {
            if (self is UITaskEventP2<P1, P2> uiTaskEventParam)
            {
                return uiTaskEventParam.Add(callback);
            }
            else
            {
                Logger.LogError(
                    $"添加 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return null;
        }

        public static UITaskEventHandleP3<P1, P2, P3> AddTaskEvent<P1, P2, P3>(this UIEventBase            self,
                                                                       UITaskEventDelegate<P1, P2, P3> callback)
        {
            if (self is UITaskEventP3<P1, P2, P3> uiTaskEventParam)
            {
                return uiTaskEventParam.Add(callback);
            }
            else
            {
                Logger.LogError(
                    $"添加 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return null;
        }

        public static UITaskEventHandleP4<P1, P2, P3, P4> AddTaskEvent<P1, P2, P3, P4>(
            this UIEventBase self, UITaskEventDelegate<P1, P2, P3, P4> callback)
        {
            if (self is UITaskEventP4<P1, P2, P3, P4> uiTaskEventParam)
            {
                return uiTaskEventParam.Add(callback);
            }
            else
            {
                Logger.LogError(
                    $"添加 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} {typeof(P4).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return null;
        }

        public static UITaskEventHandleP5<P1, P2, P3, P4, P5> AddTaskEvent<P1, P2, P3, P4, P5>(
            this UIEventBase self, UITaskEventDelegate<P1, P2, P3, P4, P5> callback)
        {
            if (self is UITaskEventP5<P1, P2, P3, P4, P5> uiTaskEventParam)
            {
                return uiTaskEventParam.Add(callback);
            }
            else
            {
                Logger.LogError(
                    $"添加 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} {typeof(P4).Name} {typeof(P5).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return null;
        }

        #endregion

        #region 移除事件

        public static bool RemoveTaskEvent(this UIEventBase self, UITaskEventHandleP0 handle)
        {
            if (self is UITaskEventP0 uiTaskEventParam)
            {
                return uiTaskEventParam.Remove(handle);
            }
            else
            {
                Logger.LogError($"移除 当前事件 {self.EventName} 参数不一致 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return false;
        }

        public static bool RemoveTaskEvent<P1>(this UIEventBase self, UITaskEventHandleP1<P1> handle)
        {
            if (self is UITaskEventP1<P1> uiTaskEventParam)
            {
                return uiTaskEventParam.Remove(handle);
            }
            else
            {
                Logger.LogError(
                    $"移除 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return false;
        }

        public static bool RemoveTaskEvent<P1, P2>(this UIEventBase self, UITaskEventHandleP2<P1, P2> handle)
        {
            if (self is UITaskEventP2<P1, P2> uiTaskEventParam)
            {
                return uiTaskEventParam.Remove(handle);
            }
            else
            {
                Logger.LogError(
                    $"移除 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return false;
        }

        public static bool RemoveTaskEvent<P1, P2, P3>(this UIEventBase self, UITaskEventHandleP3<P1, P2, P3> handle)
        {
            if (self is UITaskEventP3<P1, P2, P3> uiTaskEventParam)
            {
                return uiTaskEventParam.Remove(handle);
            }
            else
            {
                Logger.LogError(
                    $"移除 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return false;
        }

        public static bool RemoveTaskEvent<P1, P2, P3, P4>(this UIEventBase self, UITaskEventHandleP4<P1, P2, P3, P4> handle)
        {
            if (self is UITaskEventP4<P1, P2, P3, P4> uiTaskEventParam)
            {
                return uiTaskEventParam.Remove(handle);
            }
            else
            {
                Logger.LogError(
                    $"移除 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} {typeof(P4).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return false;
        }

        public static bool RemoveTaskEvent<P1, P2, P3, P4, P5>(this UIEventBase                    self,
                                                           UITaskEventHandleP5<P1, P2, P3, P4, P5> handle)
        {
            if (self is UITaskEventP5<P1, P2, P3, P4, P5> uiTaskEventParam)
            {
                return uiTaskEventParam.Remove(handle);
            }
            else
            {
                Logger.LogError(
                    $"移除 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} {typeof(P4).Name} {typeof(P5).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return false;
        }

        #endregion
    }
}