using System;
using System.Collections.Generic;
using System.Text;

namespace CLRSharp
{
    //一个ICLRType 是一个所有类型的抽象，无论是System.Type
    //还是CLRSharp的抽象，均可通过ICLRType进行调用
    public interface ICLRType
    {
        ICLRSharp_Environment env
        {
            get;
        }
        string Name
        {
            get;
        }
        string FullName
        {
            get;
        }
        string FullNameWithAssembly
        {
            get;
        }
        System.Type TypeForSystem
        {
            get;
        }
        //funcname==".ctor" 表示构造函数
        IMethod GetMethod(string funcname, MethodParamList types);
        IMethod[] GetMethods(string funcname);
        IMethod[] GetAllMethods();
        object InitObj();
        /// <summary>
        /// 获取模板函数
        /// </summary>
        /// <param name="funcname"></param>
        /// <param name="TTypes"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        IMethod GetMethodT(string funcname, MethodParamList TTypes, MethodParamList types);
        IField GetField(string name);
        string[] GetFieldNames();
        bool IsInst(object obj);

        ICLRType GetNestType(ICLRSharp_Environment env, string fullname);
        ICLRType[] SubTypes
        {
            get;
        }

        bool IsEnum();
            
    }
    public interface ICLRType_Sharp : ICLRType
    {
        CLRSharp_Instance staticInstance
        {
            get;
        }
        void ResetStaticInstace();
        bool NeedCCtor
        {
            get;
        }
        void InvokeCCtor(ThreadContext context);
        Mono.Cecil.TypeDefinition type_CLRSharp
        {
            get;
        }
        IMethod GetVMethod(IMethod _base);

        bool ContainBase(Type t);
        bool HasSysBase
        {
            get;
        }
        string[] GetMethodNames();
    }
    public interface ICLRType_System : ICLRType
    {
        Delegate CreateDelegate(Type deletype, object _this, IMethod_System _method);
    }
    public interface IMethod
    {
        object Invoke(ThreadContext context, object _this, object[] _params);
        object Invoke(ThreadContext context, object _this, object[] _params,bool bVisual);

        object Invoke(ThreadContext context, object _this, object[] _params, bool bVisual,bool autoLogDump);

        bool isStatic
        {
            get;
        }
        string Name
        {
            get;
        }

        ICLRType DeclaringType
        {
            get;
        }
        ICLRType ReturnType
        {
            get;
        }
        MethodParamList ParamList
        {
            get;
        }
    }
    public interface IMethod_System : IMethod
    {
        System.Reflection.MethodBase method_System
        {
            get;
        }
    }
    public interface IMethod_Sharp : IMethod
    {
        CodeBody body
        {
            get;
        }

    }
    public interface IField
    {
        void Set(object _this, object value);
        object Get(object _this);
        bool isStatic
        {
            get;
        }
        ICLRType DeclaringType
        {
            get;

        }
        ICLRType FieldType
        {
            get;
        }
    }

}
