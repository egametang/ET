using System;
using System.Collections.Generic;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    public static class UIEventBaseHelper
    {
        #region 创建

        public static UIEventBase CreatorUIEventBase(string name, List<EUIEventParamType> paramList)
        {
            UIEventBase uiEventBase = null;
            switch (paramList.Count)
            {
                case 0:
                    uiEventBase = CreatorUIEventP0();
                    break;
                case 1:
                    uiEventBase = CreatorUIEventP1(paramList);
                    break;
                case 2:
                    uiEventBase = CreatorUIEventP2(paramList);
                    break;
                case 3:
                    uiEventBase = CreatorUIEventP3(paramList);
                    break;
                case 4:
                    uiEventBase = CreatorUIEventP4(paramList);
                    break;
                case 5:
                    uiEventBase = CreatorUIEventP5(paramList);
                    break;
                default:
                    Logger.LogError($"最高支持5个参数 如果还想支持就自己扩展 {paramList.Count}");
                    break;
            }

            if (uiEventBase == null)
            {
                Logger.LogError($"{name} 动态创建泛型事件 失败 请检查原因");
                return null;
            }

            uiEventBase.SetName(name);
            uiEventBase.RefreshAllEventParamType(paramList);
            return uiEventBase;
        }

        private static UIEventBase CreatorUIEventP0()
        {
            return (UIEventBase)(Activator.CreateInstance(typeof(UIEventP0)));
        }

        private static UIEventBase CreatorUIEventP1(List<EUIEventParamType> paramList)
        {
            return (UIEventBase)(Activator.CreateInstance(typeof(UIEventP1<>)
                .MakeGenericType(
                    paramList[0].GetParamType()
                )));
        }

        private static UIEventBase CreatorUIEventP2(List<EUIEventParamType> paramList)
        {
            return (UIEventBase)(Activator.CreateInstance(typeof(UIEventP2<,>)
                .MakeGenericType(
                    paramList[0].GetParamType(),
                    paramList[1].GetParamType()
                )));
        }

        private static UIEventBase CreatorUIEventP3(List<EUIEventParamType> paramList)
        {
            return (UIEventBase)(Activator.CreateInstance(typeof(UIEventP3<,,>)
                .MakeGenericType(
                    paramList[0].GetParamType(),
                    paramList[1].GetParamType(),
                    paramList[2].GetParamType()
                )));
        }

        private static UIEventBase CreatorUIEventP4(List<EUIEventParamType> paramList)
        {
            return (UIEventBase)(Activator.CreateInstance(typeof(UIEventP4<,,,>)
                .MakeGenericType(
                    paramList[0].GetParamType(),
                    paramList[1].GetParamType(),
                    paramList[2].GetParamType(),
                    paramList[3].GetParamType()
                )));
        }

        private static UIEventBase CreatorUIEventP5(List<EUIEventParamType> paramList)
        {
            return (UIEventBase)(Activator.CreateInstance(typeof(UIEventP5<,,,,>)
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

        public static void Invoke(this UIEventBase self)
        {
            if (self is IUIEventInvoke uiEventInvoke)
            {
                uiEventInvoke.Invoke();
            }
            else
            {
                Logger.LogError($"调用 当前事件 {self.EventName} 参数不一致 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }
        }

        public static void Invoke<P1>(this UIEventBase self, P1 p1)
        {
            if (self is IUIEventInvoke<P1> uiEventInvoke)
            {
                uiEventInvoke.Invoke(p1);
            }
            else
            {
                Logger.LogError(
                    $"调用 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }
        }

        public static void Invoke<P1, P2>(this UIEventBase self, P1 p1, P2 p2)
        {
            if (self is IUIEventInvoke<P1, P2> uiEventInvoke)
            {
                uiEventInvoke.Invoke(p1, p2);
            }
            else
            {
                Logger.LogError(
                    $"调用 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }
        }

        public static void Invoke<P1, P2, P3>(this UIEventBase self, P1 p1, P2 p2, P3 p3)
        {
            if (self is IUIEventInvoke<P1, P2, P3> uiEventInvoke)
            {
                uiEventInvoke.Invoke(p1, p2, p3);
            }
            else
            {
                Logger.LogError(
                    $"调用 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }
        }

        public static void Invoke<P1, P2, P3, P4>(this UIEventBase self, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            if (self is IUIEventInvoke<P1, P2, P3, P4> uiEventInvoke)
            {
                uiEventInvoke.Invoke(p1, p2, p3, p4);
            }
            else
            {
                Logger.LogError(
                    $"调用 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} {typeof(P4).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }
        }

        public static void Invoke<P1, P2, P3, P4, P5>(this UIEventBase self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            if (self is IUIEventInvoke<P1, P2, P3, P4, P5> uiEventInvoke)
            {
                uiEventInvoke.Invoke(p1, p2, p3, p4, p5);
            }
            else
            {
                Logger.LogError(
                    $"调用 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} {typeof(P4).Name} {typeof(P5).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }
        }

        #endregion

        #region 添加事件

        public static UIEventHandleP0 AddEvent(this UIEventBase self, UIEventDelegate callback)
        {
            if (self is UIEventP0 uiEventParam)
            {
                return uiEventParam.Add(callback);
            }
            else
            {
                Logger.LogError($"添加 当前事件 {self.EventName} 参数不一致 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return null;
        }

        public static UIEventHandleP1<P1> AddEvent<P1>(this UIEventBase self, UIEventDelegate<P1> callback)
        {
            if (self is UIEventP1<P1> uiEventParam)
            {
                return uiEventParam.Add(callback);
            }
            else
            {
                Logger.LogError(
                    $"添加 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return null;
        }

        public static UIEventHandleP2<P1, P2> AddEvent<P1, P2>(this UIEventBase self, UIEventDelegate<P1, P2> callback)
        {
            if (self is UIEventP2<P1, P2> uiEventParam)
            {
                return uiEventParam.Add(callback);
            }
            else
            {
                Logger.LogError(
                    $"添加 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return null;
        }

        public static UIEventHandleP3<P1, P2, P3> AddEvent<P1, P2, P3>(this UIEventBase            self,
                                                                       UIEventDelegate<P1, P2, P3> callback)
        {
            if (self is UIEventP3<P1, P2, P3> uiEventParam)
            {
                return uiEventParam.Add(callback);
            }
            else
            {
                Logger.LogError(
                    $"添加 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return null;
        }

        public static UIEventHandleP4<P1, P2, P3, P4> AddEvent<P1, P2, P3, P4>(
            this UIEventBase self, UIEventDelegate<P1, P2, P3, P4> callback)
        {
            if (self is UIEventP4<P1, P2, P3, P4> uiEventParam)
            {
                return uiEventParam.Add(callback);
            }
            else
            {
                Logger.LogError(
                    $"添加 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} {typeof(P4).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return null;
        }

        public static UIEventHandleP5<P1, P2, P3, P4, P5> AddEvent<P1, P2, P3, P4, P5>(
            this UIEventBase self, UIEventDelegate<P1, P2, P3, P4, P5> callback)
        {
            if (self is UIEventP5<P1, P2, P3, P4, P5> uiEventParam)
            {
                return uiEventParam.Add(callback);
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

        public static bool RemoveEvent(this UIEventBase self, UIEventHandleP0 handle)
        {
            if (self is UIEventP0 uiEventParam)
            {
                return uiEventParam.Remove(handle);
            }
            else
            {
                Logger.LogError($"移除 当前事件 {self.EventName} 参数不一致 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return false;
        }

        public static bool RemoveEvent<P1>(this UIEventBase self, UIEventHandleP1<P1> handle)
        {
            if (self is UIEventP1<P1> uiEventParam)
            {
                return uiEventParam.Remove(handle);
            }
            else
            {
                Logger.LogError(
                    $"移除 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return false;
        }

        public static bool RemoveEvent<P1, P2>(this UIEventBase self, UIEventHandleP2<P1, P2> handle)
        {
            if (self is UIEventP2<P1, P2> uiEventParam)
            {
                return uiEventParam.Remove(handle);
            }
            else
            {
                Logger.LogError(
                    $"移除 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return false;
        }

        public static bool RemoveEvent<P1, P2, P3>(this UIEventBase self, UIEventHandleP3<P1, P2, P3> handle)
        {
            if (self is UIEventP3<P1, P2, P3> uiEventParam)
            {
                return uiEventParam.Remove(handle);
            }
            else
            {
                Logger.LogError(
                    $"移除 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return false;
        }

        public static bool RemoveEvent<P1, P2, P3, P4>(this UIEventBase self, UIEventHandleP4<P1, P2, P3, P4> handle)
        {
            if (self is UIEventP4<P1, P2, P3, P4> uiEventParam)
            {
                return uiEventParam.Remove(handle);
            }
            else
            {
                Logger.LogError(
                    $"移除 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} {typeof(P4).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return false;
        }

        public static bool RemoveEvent<P1, P2, P3, P4, P5>(this UIEventBase                    self,
                                                           UIEventHandleP5<P1, P2, P3, P4, P5> handle)
        {
            if (self is UIEventP5<P1, P2, P3, P4, P5> uiEventParam)
            {
                return uiEventParam.Remove(handle);
            }
            else
            {
                Logger.LogError(
                    $"移除 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} {typeof(P4).Name} {typeof(P5).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return false;
        }

        #endregion

        #region 判断

        public static bool ParamEquals(this UIEventBase self, List<EUIEventParamType> targetParamList)
        {
            return self.AllEventParamType.ParamEquals(targetParamList);
        }

        public static bool ParamEquals(this List<EUIEventParamType> self, List<EUIEventParamType> target)
        {
            if (self == null || target == null)
            {
                return false;
            }

            if (self.Count == 0 && target.Count == 0)
            {
                return true;
            }

            if (self.Count != target.Count)
            {
                return false; //长度都不一致还检查什么
            }

            for (var i = 0; i < self.Count; i++)
            {
                var selfParam   = self[i];
                var targetParam = target[i];
                if (selfParam != targetParam)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region 其他

        public static List<string> GetAllParamTypeToListString(this List<EUIEventParamType> self)
        {
            var list = new List<string>();
            foreach (var paramType in self)
            {
                list.Add(paramType.ToString());
            }

            return list;
        }

        public static string GetAllParamTypeToString(this List<EUIEventParamType> self)
        {
            var value = "";

            foreach (var paramType in self)
            {
                value += $" {paramType}";
            }

            return value;
        }

        public static string GetAllParamTypeTips(this List<EUIEventParamType> self)
        {
            if (self == null)
            {
                return "";
            }

            var value = $"参数个数:{self.Count}";

            foreach (var paramType in self)
            {
                value += $" , {paramType}";
            }

            return value;
        }

        #endregion
    }
}