using System;
using System.Collections.Generic;
using System.Text;

namespace CLRSharp
{
    /// <summary>
    /// 委托绑定
    /// </summary>
    public class Delegate_Binder
    {
        static Dictionary<Type, IDelegate_BindTool> mapBind = new Dictionary<Type, IDelegate_BindTool>();
        public static void RegBind(Type deletype, IDelegate_BindTool bindtool)
        {
            mapBind[deletype] = bindtool;
        }
        public static Delegate MakeDelegate(Type deletype, CLRSharp_Instance _this_inst, IMethod __method)
        {
            IDelegate_BindTool btool = null;
            if (mapBind.TryGetValue(deletype, out btool))
            {
                return btool.CreateDele(deletype, null, _this_inst, __method);
            }
            var method = deletype.GetMethod("Invoke");
            if (__method.isStatic)
            {
                _this_inst = null;
            }
            var pp = method.GetParameters();

            if (method.ReturnType == typeof(void))
            {
                if (pp.Length == 0)
                {
                    //var gtype = typeof(Delegate_BindTool).MakeGenericType(new Type[] { });
                    btool = new Delegate_BindTool();
                }
                else if (pp.Length == 1)
                {
                    var gtype = typeof(Delegate_BindTool<>).MakeGenericType(new Type[] { pp[0].ParameterType });
                    btool = gtype.GetConstructor(new Type[] { }).Invoke(new object[] { }) as IDelegate_BindTool;
                }
                else if (pp.Length == 2)
                {
                    var gtype = typeof(Delegate_BindTool<,>).MakeGenericType(new Type[] { pp[0].ParameterType, pp[1].ParameterType });
                    btool = gtype.GetConstructor(new Type[] { }).Invoke(new object[] { }) as IDelegate_BindTool;
                }
                else if (pp.Length == 3)
                {
                    var gtype = typeof(Delegate_BindTool<,,>).MakeGenericType(new Type[] { pp[0].ParameterType, pp[1].ParameterType, pp[2].ParameterType });
                    btool = gtype.GetConstructor(new Type[] { }).Invoke(new object[] { }) as IDelegate_BindTool;
                }
                else if (pp.Length == 4)
                {
                    var gtype = typeof(Delegate_BindTool<,,,>).MakeGenericType(new Type[] { pp[0].ParameterType, pp[1].ParameterType, pp[2].ParameterType, pp[3].ParameterType });
                    btool = gtype.GetConstructor(new Type[] { }).Invoke(new object[] { }) as IDelegate_BindTool;
                }
                else
                {
                    throw new Exception("还没有支持这么多参数的委托");
                }
            }
            else
            {
                if (pp.Length == 0)
                {
                    var gtype = typeof(Delegate_BindTool_Ret<>).MakeGenericType(new Type[] { method.ReturnType });
                    btool = gtype.GetConstructor(new Type[] { }).Invoke(new object[] { }) as IDelegate_BindTool;

                }
                else if (pp.Length == 1)
                {
                    var gtype = typeof(Delegate_BindTool_Ret<,>).MakeGenericType(new Type[] { method.ReturnType, pp[0].ParameterType });
                    btool = gtype.GetConstructor(new Type[] { }).Invoke(new object[] { }) as IDelegate_BindTool;
                }
                else if (pp.Length == 2)
                {
                    var gtype = typeof(Delegate_BindTool_Ret<,,>).MakeGenericType(new Type[] { method.ReturnType, pp[0].ParameterType, pp[1].ParameterType });
                    btool = gtype.GetConstructor(new Type[] { }).Invoke(new object[] { }) as IDelegate_BindTool;
                }
                else if (pp.Length == 3)
                {
                    var gtype = typeof(Delegate_BindTool_Ret<,,,>).MakeGenericType(new Type[] { method.ReturnType, pp[0].ParameterType, pp[1].ParameterType, pp[2].ParameterType });
                    btool = gtype.GetConstructor(new Type[] { }).Invoke(new object[] { }) as IDelegate_BindTool;
                }
                else if (pp.Length == 4)
                {
                    var gtype = typeof(Delegate_BindTool_Ret<,,,>).MakeGenericType(new Type[] { method.ReturnType, pp[0].ParameterType, pp[1].ParameterType, pp[2].ParameterType, pp[3].ParameterType });
                    btool = gtype.GetConstructor(new Type[] { }).Invoke(new object[] { }) as IDelegate_BindTool;
                }
                else
                {
                    throw new Exception("还没有支持这么多参数的委托");
                }
            }
            mapBind[deletype] = btool;
            return btool.CreateDele(deletype, null, _this_inst, __method);
        }
    }
    public interface IDelegate_BindTool
    {
        Delegate CreateDele(Type deletype, ThreadContext context, CLRSharp_Instance _this, IMethod _method);
    }
    public class Delegate_BindTool : IDelegate_BindTool
    {
        delegate void Action();
        public Delegate CreateDele(Type deletype, ThreadContext context, CLRSharp_Instance _this, IMethod _method)
        {
            Action act = () =>
            {
                _method.Invoke(context, _this, new object[] { }, true, true);
            };
            return Delegate.CreateDelegate(deletype, act.Target, act.Method);
        }
    }
    public class Delegate_BindTool<T1> : IDelegate_BindTool
    {
        delegate void Action(T1 p1);
        public Delegate CreateDele(Type deletype, ThreadContext context, CLRSharp_Instance _this, IMethod _method)
        {
            Action act = (p1) =>
            {
                _method.Invoke(context, _this, new object[] { p1 }, true, true);
            };
            return Delegate.CreateDelegate(deletype, act.Target, act.Method);
        }
    }
    public class Delegate_BindTool<T1, T2> : IDelegate_BindTool
    {
        delegate void Action(T1 p1, T2 p2);
        public Delegate CreateDele(Type deletype, ThreadContext context, CLRSharp_Instance _this, IMethod _method)
        {
            Action act = (p1, p2) =>
            {
                _method.Invoke(context, _this, new object[] { p1, p2 }, true, true);
            };
            return Delegate.CreateDelegate(deletype, act.Target, act.Method);
        }
    }
    public class Delegate_BindTool<T1, T2, T3> : IDelegate_BindTool
    {
        delegate void Action(T1 p1, T2 p2, T3 p3);
        public Delegate CreateDele(Type deletype, ThreadContext context, CLRSharp_Instance _this, IMethod _method)
        {
            Action act = (p1, p2, p3) =>
            {
                _method.Invoke(context, _this, new object[] { p1, p2, p3 }, true, true);
            };
            return Delegate.CreateDelegate(deletype, act.Target, act.Method);
        }
    }
    public class Delegate_BindTool<T1, T2, T3, T4> : IDelegate_BindTool
    {
        delegate void Action(T1 p1, T2 p2, T3 p3, T4 p4);
        public Delegate CreateDele(Type deletype, ThreadContext context, CLRSharp_Instance _this, IMethod _method)
        {
            Action act = (p1, p2, p3, p4) =>
            {
                _method.Invoke(context, _this, new object[] { p1, p2, p3, p4 }, true, true);
            };
            return Delegate.CreateDelegate(deletype, act.Target, act.Method);
        }
    }
    public class Delegate_BindTool_Ret<TRet> : IDelegate_BindTool
    {
        delegate TRet Action();
        public Delegate CreateDele(Type deletype, ThreadContext context, CLRSharp_Instance _this, IMethod _method)
        {
            Action act = () =>
            {
                var o = _method.Invoke(context, _this, new object[] { }, true, true);
                if (o is VBox)
                {
                    o = (o as VBox).BoxDefine();
                }
                return (TRet)o;
            };
            return Delegate.CreateDelegate(deletype, act.Target, act.Method);
        }
    }
    public class Delegate_BindTool_Ret<TRet, T1> : IDelegate_BindTool
    {
        delegate TRet Action(T1 p1);
        public Delegate CreateDele(Type deletype, ThreadContext context, CLRSharp_Instance _this, IMethod _method)
        {
            Action act = (T1 p1) =>
            {
                var o = _method.Invoke(context, _this, new object[] { p1 }, true, true);
                if (o is VBox)
                {
                    o = (o as VBox).BoxDefine();
                }
                return (TRet)o;
            };
            return Delegate.CreateDelegate(deletype, act.Target, act.Method);
        }
    }
    public class Delegate_BindTool_Ret<TRet, T1, T2> : IDelegate_BindTool
    {
        delegate TRet Action(T1 p1, T2 p2);
        public Delegate CreateDele(Type deletype, ThreadContext context, CLRSharp_Instance _this, IMethod _method)
        {
            Action act = (T1 p1, T2 p2) =>
            {
                var o = _method.Invoke(context, _this, new object[] { p1, p2 }, true, true);
                if (o is VBox)
                {
                    o = (o as VBox).BoxDefine();
                }
                return (TRet)o;
            };
            return Delegate.CreateDelegate(deletype, act.Target, act.Method);
        }
    }
    public class Delegate_BindTool_Ret<TRet, T1, T2, T3> : IDelegate_BindTool
    {
        delegate TRet Action(T1 p1, T2 p2, T3 p3);
        public Delegate CreateDele(Type deletype, ThreadContext context, CLRSharp_Instance _this, IMethod _method)
        {
            Action act = (T1 p1, T2 p2, T3 p3) =>
            {
                object o = _method.Invoke(context, _this, new object[] { p1, p2, p3 }, true, true);
                if (o is VBox)
                {
                    o = (o as VBox).BoxDefine();
                }
                return (TRet)o;
            };
            return Delegate.CreateDelegate(deletype, act.Target, act.Method);
        }
    }
    public class Delegate_BindTool_Ret<TRet, T1, T2, T3, T4> : IDelegate_BindTool
    {
        delegate TRet Action(T1 p1, T2 p2, T3 p3, T4 p4);
        public Delegate CreateDele(Type deletype, ThreadContext context, CLRSharp_Instance _this, IMethod _method)
        {
            Action act = (T1 p1, T2 p2, T3 p3, T4 p4) =>
            {
                object o = _method.Invoke(context, _this, new object[] { p1, p2, p3, p4 }, true, true);
                if (o is VBox)
                {
                    o = (o as VBox).BoxDefine();
                }
                return (TRet)o;
            };
            return Delegate.CreateDelegate(deletype, act.Target, act.Method);
        }
    }
}
