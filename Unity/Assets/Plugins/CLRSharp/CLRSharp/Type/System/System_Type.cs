using System;
using System.Collections.Generic;
using System.Text;

namespace CLRSharp
{
    public class Type_Common_System : ICLRType_System
    {
        public System.Type TypeForSystem
        {
            get;
            private set;
        }
        public ICLRSharp_Environment env
        {
            get;
            private set;
        }
        public ICLRType[] SubTypes
        {
            get;
            private set;
        }
        public Type_Common_System(ICLRSharp_Environment env, System.Type type, ICLRType[] subtype)
        {
            this.env = env;
            this.TypeForSystem = type;
            FullNameWithAssembly = type.AssemblyQualifiedName;
            this.SubTypes = subtype;
        }
        public string Name
        {
            get { return TypeForSystem.Name; }
        }

        public string FullName
        {
            get { return TypeForSystem.FullName; }
        }
        public string FullNameWithAssembly
        {
            get;
            private set;

            //{
            //    string aname = TypeForSystem.AssemblyQualifiedName;
            //    int i = aname.IndexOf(',');
            //    i = aname.IndexOf(',', i + 1);
            //    return aname.Substring(0, i);
            //}
        }
        public virtual IMethod GetMethod(string funcname, MethodParamList types)
        {
            if (funcname == ".ctor")
            {
                var con = TypeForSystem.GetConstructor(types.ToArraySystem());
                return new Method_Common_System(this, con);
            }
            var method = TypeForSystem.GetMethod(funcname, types.ToArraySystem());
            return new Method_Common_System(this, method);
        }
        public virtual IMethod[] GetMethods(string funcname)
        {
            List<IMethod> methods = new List<IMethod>();
            if (funcname == ".ctor")
            {
                var cons = TypeForSystem.GetConstructors();
                foreach (var c in cons)
                {
                    methods.Add(new Method_Common_System(this, c));
                }

            }
            else
            {
                var __methods = TypeForSystem.GetMethods();
                foreach (var m in __methods)
                {
                    if (m.Name == funcname)
                    {
                        methods.Add(new Method_Common_System(this, m));
                    }
                }
            }

            return methods.ToArray();
        }
        public virtual IMethod[] GetAllMethods()
        {
            List<IMethod> methods = new List<IMethod>();
            {
                var __methods = TypeForSystem.GetMethods();
                foreach (var m in __methods)
                {
                    //if (m.Name == funcname)
                    {
                        methods.Add(new Method_Common_System(this, m));
                    }
                }
            }

            return methods.ToArray();
        }
        public object InitObj()
        {
            return Activator.CreateInstance(TypeForSystem);
        }
        public virtual IMethod GetMethodT(string funcname, MethodParamList ttypes, MethodParamList types)
        {
            //这个实现还不完全
            //有个别重构下，判定比这个要复杂
            System.Reflection.MethodInfo _method = null;
            var ms = TypeForSystem.GetMethods();
            foreach (var m in ms)
            {
                if (m.Name == funcname && m.IsGenericMethodDefinition)
                {
                    var ts = m.GetGenericArguments();
                    var ps = m.GetParameters();
                    if (ts.Length == ttypes.Count && ps.Length == types.Count)
                    {
                        _method = m;
                        break;
                    }

                }
            }

            // _method = TypeForSystem.GetMethod(funcname, types.ToArraySystem());

            return new Method_Common_System(this, _method.MakeGenericMethod(ttypes.ToArraySystem()));
        }
        public virtual IField GetField(string name)
        {
            return new Field_Common_System(env, TypeForSystem.GetField(name));
        }
        public bool IsInst(object obj)
        {
            return TypeForSystem.IsInstanceOfType(obj);

        }


        public ICLRType GetNestType(ICLRSharp_Environment env, string fullname)
        {
            throw new NotImplementedException();
        }

        public Delegate CreateDelegate(Type deletype, object _this, IMethod_System _method)
        {
            return Delegate.CreateDelegate(deletype, _this, _method.method_System as System.Reflection.MethodInfo);
        }


        public string[] GetFieldNames()
        {
            var fs = TypeForSystem.GetFields();
            string[] names = new string[fs.Length];
            for (int i = 0; i < fs.Length; i++)
            {
                names[i] = fs[i].Name;
            }
            return names;
        }
        public bool IsEnum()
        {
            return TypeForSystem.IsEnum;
        }
    }
    class Field_Common_System : IField
    {
        public System.Reflection.FieldInfo info;
        public Field_Common_System(ICLRSharp_Environment env, System.Reflection.FieldInfo field)
        {
            info = field;

            FieldType = env.GetType(field.FieldType);
            DeclaringType = env.GetType(field.DeclaringType);
        }
        public ICLRType FieldType
        {
            get;
            private set;
        }
        public ICLRType DeclaringType
        {
            get;
            private set;
        }
        public void Set(object _this, object value)
        {
            if(value!=null&&(value.GetType()==typeof(int)|| value.GetType() == typeof(Int64)))
            {
                if (info.FieldType == typeof(bool))
                    value = (bool)((int)value != 0);
                else if(info.FieldType==typeof(char))
                {
                    value = (char)((int)value);
                }
                else if (info.FieldType == typeof(byte))
                {
                    value = (byte)((int)value);
                }
                else if (info.FieldType == typeof(sbyte))
                {
                    value = (sbyte)((int)value);
                }
                else if (info.FieldType == typeof(UInt16))
                {
                    value = (UInt16)((int)value);
                }
                else if (info.FieldType == typeof(Int16))
                {
                    value = (Int16)((int)value);
                }
                else if (info.FieldType == typeof(UInt32))
                {
                    value = (UInt32)((int)value);
                }
                else if (info.FieldType == typeof(UInt64))
                {
                    value = (UInt64)((Int64)value);
                }

            }
         
            info.SetValue(_this, value);
        }

        public object Get(object _this)
        {
            return info.GetValue(_this);
        }

        public bool isStatic
        {
            get { return info.IsStatic; }
        }
    }

    class Method_Common_System : IMethod_System
    {

        public Method_Common_System(ICLRType DeclaringType, System.Reflection.MethodBase method)
        {
            if (method == null)
                throw new Exception("not allow null method.");
            method_System = method;
            this.DeclaringType = DeclaringType;
            if (method is System.Reflection.MethodInfo)
            {
                System.Reflection.MethodInfo info = method as System.Reflection.MethodInfo;
                ReturnType = DeclaringType.env.GetType(info.ReturnType);
            }
            ParamList = new MethodParamList(DeclaringType.env, method);
        }
        public bool isStatic
        {
            get { return method_System.IsStatic; }
        }
        public string Name
        {
            get
            {
                return method_System.Name;
            }
        }

        public ICLRType DeclaringType
        {
            get;
            private set;

        }
        public ICLRType ReturnType
        {
            get;
            private set;
        }
        public MethodParamList ParamList
        {
            get;
            private set;
        }
        public System.Reflection.MethodBase method_System
        {
            get;
            private set;
        }
        public object Invoke(ThreadContext context, object _this, object[] _params, bool bVisual)
        {//对程序类型，其实我们做不到区分虚实调用。。。没办法
            if (this.Name == "Concat" && this.DeclaringType.TypeForSystem == typeof(string))
            {//这里有一个IL2CPP的问题


                if (_params.Length == 1)
                {
                    if (_params[0] == null)
                        return "null";
                    if (_params[0] is string[])
                    {
                        return string.Concat(_params[0] as string[]);
                    }
                    else if (_params[0] is object[])
                    {
                        return string.Concat(_params[0] as object[]);
                    }
                    else
                    {
                        return _params[0].ToString();
                    }
                }
                else
                {
                    string outstr = "null";
                    if (_params[0] != null) outstr = _params[0].ToString();

                    for (int i = 1; i < _params.Length; i++)
                    {
                        if (_params[i] != null)
                            outstr += _params[i];
                        else
                            outstr += "null";
                    }
                    return outstr;
                }

            }
            return Invoke(context, _this, _params);
        }
        public object Invoke(ThreadContext context, object _this, object[] _params)
        {
            if (_this is CLRSharp_Instance)
            {
                CLRSharp_Instance inst = _this as CLRSharp_Instance;
                if (inst.type.HasSysBase)
                {
                    var btype = inst.type.ContainBase(method_System.DeclaringType);
                    if (btype)
                    {
                        var CrossBind = context.environment.GetCrossBind(method_System.DeclaringType);
                        if (CrossBind != null)
                        {
                            _this = CrossBind.CreateBind(inst);
                        }
                        else
                        {
                            _this = (_this as CLRSharp_Instance).system_base;
                            //如果没有绑定器，尝试直接使用System_base;
                        }
                        //context.environment.logger.Log("这里有一个需要映射的类型");
                    }
                }
            }
            //委托是很特殊的存在
            //if(this.DeclaringType.IsDelegate)
            //{

            //}
            if (method_System is System.Reflection.ConstructorInfo)
            {
                if (method_System.DeclaringType.IsSubclassOf(typeof(Delegate)))
                {//创建委托
                    object src = _params[0];
                    RefFunc fun = _params[1] as RefFunc;
                    ICLRType_Sharp clrtype = fun._method.DeclaringType as ICLRType_Sharp;
                    if (clrtype != null)//onclr
                    {

                        CLRSharp_Instance inst = src as CLRSharp_Instance;
                        if (fun._method.isStatic && clrtype != null)
                            inst = clrtype.staticInstance;
                        return inst.GetDelegate(context, method_System.DeclaringType, fun._method);
                    }
                    else//onsystem
                    {
                        ICLRType_System stype = fun._method.DeclaringType as ICLRType_System;
                        return stype.CreateDelegate(method_System.DeclaringType, src, fun._method as IMethod_System);
                    }
                }
                object[] _outp = null;
                if (_params != null && _params.Length > 0)
                {
                    _outp = new object[_params.Length];
                    var _paramsdef = method_System.GetParameters();
                    for (int i = 0; i < _params.Length; i++)
                    {
                        if (_params[i] == null)
                        {
                            _outp[i] = null;
                            continue;
                        }
                        Type tsrc = _params[i].GetType();
                        Type ttarget = _paramsdef[i].ParameterType;
                        if (tsrc == ttarget)
                        {
                            _outp[i] = _params[i];
                        }
                        else if (tsrc.IsSubclassOf(ttarget))
                        {
                            _outp[i] = _params[i];
                        }
                        else if (_paramsdef[i].ParameterType.IsEnum)//特殊处理枚举
                        {
                            var ms = _paramsdef[i].ParameterType.GetMethods();
                            _outp[i] = Enum.ToObject(_paramsdef[i].ParameterType, _params[i]);
                        }
                        else
                        {
                            if (ttarget == typeof(byte))
                                _outp[i] = (byte)Convert.ToDecimal(_params[i]);
                            else
                            {
                                _outp[i] = _params[i];
                            }
                            //var ms =_params[i].GetType().GetMethods();
                        }
                    }
                }
                var newobj = (method_System as System.Reflection.ConstructorInfo).Invoke(_outp);
                return newobj;
            }
            else
            {
                Dictionary<int, object> hasref = new Dictionary<int, object>();
                object[] _outp = null;
                if (_params != null && _params.Length > 0)
                {
                    _outp = new object[_params.Length];
                    var _paramsdef = method_System.GetParameters();
                    for (int i = 0; i < _params.Length; i++)
                    {
                        if (_params[i] is CLRSharp.StackFrame.RefObj)//特殊处理outparam
                        {
                            object v = (_params[i] as CLRSharp.StackFrame.RefObj).Get();
                            if (v is VBox)
                            {
                                v = (v as VBox).BoxDefine();
                            }
                            hasref[i] = v;
                            _outp[i] = v;
                        }
                        else if (_paramsdef[i].ParameterType.IsEnum)//特殊处理枚举
                        {
                            var ms = _paramsdef[i].ParameterType.GetMethods();
                            _outp[i] = Enum.ToObject(_paramsdef[i].ParameterType, _params[i]);
                        }
                        else
                        {
                            if(_paramsdef[i].ParameterType==typeof(UInt64)&&_params[i] is Int64)
                            {
                                _outp[i] = (UInt64)(Int64)_params[i];
                            }
                            else if (_paramsdef[i].ParameterType == typeof(Int64) && _params[i] is UInt64)
                            {
                                _outp[i] = (Int64)(UInt64)_params[i];
                            }
                            else if (_paramsdef[i].ParameterType == typeof(UInt32) && _params[i] is Int32)
                            {
                                _outp[i] = (UInt32)(Int32)_params[i];
                            }
                            else if (_paramsdef[i].ParameterType == typeof(Int32) && _params[i] is UInt32)
                            {
                                _outp[i] = (Int32)(UInt32)_params[i];
                            }
                            else
                            {
                                _outp[i] = _params[i];
                            }
                        }
                    }
                }
                //if (method_System.DeclaringType.IsSubclassOf(typeof(Delegate)))//直接用Delegate.Invoke,会导致转到本机代码再回来
                ////会导致错误堆栈不方便观察,但是也没办法直接调用，只能手写一些常用类型
                //{
                //    //需要从Delegate转换成实际类型执行的帮助类
                //    Action<int> abc = _this as Action<int>;
                //    abc((int)_params[0]);
                //    return null;
                //}
                //else
                {
                    var _out = method_System.Invoke(_this, _outp);
                    foreach (var _ref in hasref)
                    {
                        if (_ref.Value is VBox)
                        {
                            (_ref.Value as VBox).SetDirect(_outp[_ref.Key]);
                        }
                        else
                        {
                            (_params[_ref.Key] as CLRSharp.StackFrame.RefObj).Set(_outp[_ref.Key]);
                        }
                    }
                    return _out;
                }
            }

        }

        public object Invoke(ThreadContext context, object _this, object[] _params, bool bVisual, bool autoLogDump)
        {
            try
            {
                return Invoke(context, _this, _params);
            }
            catch (Exception err)
            {
                if (context == null) context = ThreadContext.activeContext;
                if (context == null)
                    throw new Exception("当前线程没有创建ThreadContext,无法Dump", err);
                else
                {
                    context.environment.logger.Log_Error("Error InSystemCall:" + this.DeclaringType.FullName + "::" + this.Name);
                    throw err;
                }
            }
        }

    }

}
