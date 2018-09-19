using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntime.Runtime.Enviorment
{
    public interface CrossBindingAdaptorType
    {
        ILTypeInstance ILInstance { get; }
    }
    /// <summary>
    /// This interface is used for inheritance and implementation of CLR Types or interfaces
    /// </summary>
    public abstract class CrossBindingAdaptor : IType
    {
        IType type;
        /// <summary>
        /// This returns the CLR type to be inherited or CLR interface to be implemented
        /// </summary>
        public abstract Type BaseCLRType { get; }

        /// <summary>
        /// If this Adaptor is capable to impelement multuple interfaces, use this Property, AND BaseCLRType should return null
        /// </summary>
        public virtual Type[] BaseCLRTypes
        {
            get
            {
                return null;
            }
        }

        public abstract Type AdaptorType { get; }

        public abstract object CreateCLRInstance(Enviorment.AppDomain appdomain, ILTypeInstance instance);

        internal IType RuntimeType { get { return type; } set { type = value; } }

        #region IType Members

        public IMethod GetMethod(string name, int paramCount, bool declaredOnly = false)
        {
            return type.GetMethod(name, paramCount, declaredOnly);
        }

        public IMethod GetMethod(string name, List<IType> param, IType[] genericArguments, IType returnType = null, bool declaredOnly = false)
        {
            return type.GetMethod(name, param, genericArguments, returnType, declaredOnly);
        }

        public List<IMethod> GetMethods()
        {
            return type.GetMethods();
        }

        public int GetFieldIndex(object token)
        {
            return type.GetFieldIndex(token);
        }

        public IMethod GetConstructor(List<IType> param)
        {
            return type.GetConstructor(param);
        }

        public bool CanAssignTo(IType type)
        {
            bool res = false;
            if (BaseType != null)
                res = BaseType.CanAssignTo(type);
            var interfaces = Implements;
            if (!res && interfaces != null)
            {
                for (int i = 0; i < interfaces.Length; i++)
                {
                    var im = interfaces[i];
                    res = im.CanAssignTo(type);
                    if (res)
                        return true;
                }
            }
            return res;
        }

        public IType MakeGenericInstance(KeyValuePair<string, IType>[] genericArguments)
        {
            return type.MakeGenericInstance(genericArguments);
        }

        public IType MakeByRefType()
        {
            return type.MakeByRefType();
        }

        public IType MakeArrayType(int rank)
        {
            return type.MakeArrayType(rank);
        }

        public IType FindGenericArgument(string key)
        {
            return type.FindGenericArgument(key);
        }

        public IType ResolveGenericType(IType contextType)
        {
            return type.ResolveGenericType(contextType);
        }

        public IMethod GetVirtualMethod(IMethod method)
        {
            return type.GetVirtualMethod(method);
        }

        public bool IsGenericInstance
        {
            get
            {
                return type.IsGenericInstance;
            }
        }

        public KeyValuePair<string, IType>[] GenericArguments
        {
            get
            {
                return type.GenericArguments;
            }
        }

        public Type TypeForCLR
        {
            get
            {
                return type.TypeForCLR;
            }
        }

        public IType ByRefType
        {
            get
            {
                return type.ByRefType;
            }
        }

        public IType ArrayType
        {
            get
            {
                return type.ArrayType;
            }
        }

        public string FullName
        {
            get
            {
                return type.FullName;
            }
        }

        public string Name
        {
            get
            {
                return type.Name;
            }
        }

        public bool IsValueType
        {
            get
            {
                return type.IsValueType;
            }
        }

        public bool IsPrimitive
        {
            get
            {
                return type.IsPrimitive;
            }
        }

        public bool IsDelegate
        {
            get
            {
                return type.IsDelegate;
            }
        }

        public AppDomain AppDomain
        {
            get
            {
                return type.AppDomain;
            }
        }

        public Type ReflectionType
        {
            get
            {
                return type.ReflectionType;
            }
        }

        public IType BaseType
        {
            get
            {
                return type.BaseType;
            }
        }

        public IType[] Implements
        {
            get
            {
                return type.Implements;
            }
        }

        public bool HasGenericParameter
        {
            get
            {
                return type.HasGenericParameter;
            }
        }

        public bool IsGenericParameter
        {
            get
            {
                return type.IsGenericParameter;
            }
        }
        public bool IsArray
        {
            get { return false; }
        }
        public bool IsByRef
        {
            get
            {
                return type.IsByRef;
            }
        }

        public bool IsInterface
        {
            get { return type.IsInterface; }
        }

        public IType ElementType
        {
            get
            {
                return type.ElementType;
            }
        }

        public int ArrayRank
        {
            get { return type.ArrayRank; }
        }
        #endregion
    }
}
