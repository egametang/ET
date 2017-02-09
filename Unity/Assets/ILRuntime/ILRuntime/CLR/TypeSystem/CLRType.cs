using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Mono.Cecil;
using ILRuntime.CLR.Method;
namespace ILRuntime.CLR.TypeSystem
{
    public class CLRType : IType
    {
        Type clrType;
        Dictionary<string, List<CLRMethod>> methods;
        ILRuntime.Runtime.Enviorment.AppDomain appdomain;
        List<CLRMethod> constructors;
        KeyValuePair<string,IType>[] genericArguments;
        List<CLRType> genericInstances;
        Dictionary<string, int> fieldMapping;
        Dictionary<int, FieldInfo> fieldInfoCache;
        Dictionary<int, int> fieldTokenMapping;
        IType byRefType, arrayType;
        bool isDelegate;
        IType baseType;
        bool isBaseTypeInitialized = false;
        MethodInfo memberwiseClone;

        public Dictionary<int, FieldInfo> Fields
        {
            get
            {
                if (fieldMapping == null)
                    InitializeFields();
                return fieldInfoCache;
            }
        }
        public ILRuntime.Runtime.Enviorment.AppDomain AppDomain
        {
            get
            {
                return appdomain;
            }
        }

        public CLRType(Type clrType, Runtime.Enviorment.AppDomain appdomain)
        {
            this.clrType = clrType;
            this.appdomain = appdomain;
            isDelegate = clrType.BaseType == typeof(MulticastDelegate);
        }
        
        public bool IsGenericInstance
        {
            get
            {
                return genericArguments != null;
            }
        }

        public KeyValuePair<string, IType>[] GenericArguments
        {
            get
            {
                return genericArguments;
            }
        }
        public bool HasGenericParameter
        {
            get
            {
                return clrType.ContainsGenericParameters;
            }
        }

        public Type TypeForCLR
        {
            get
            {
                return clrType;
            }
        }

        public Type ReflectionType
        {
            get
            {
                return clrType;
            }
        }
        public IType ByRefType
        {
            get
            {
                return byRefType;
            }
        }
        public IType ArrayType
        {
            get
            {
                return arrayType;
            }
        }
        public bool IsValueType
        {
            get
            {
                return clrType.IsValueType;
            }
        }
        public bool IsDelegate
        {
            get
            {
                return isDelegate;
            }
        }
        public string FullName
        {
            get
            {
                return clrType.FullName;
            }
        }
        public string Name
        {
            get
            {
                return clrType.Name;
            }
        }

        public IType BaseType
        {
            get
            {
                if (!isBaseTypeInitialized)
                    InitializeBaseType();
                return baseType;
            }
        }

        public new MethodInfo MemberwiseClone
        {
            get
            {
                if(clrType.IsValueType && memberwiseClone == null)
                {
                    memberwiseClone = clrType.GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                }
                return memberwiseClone;
            }
        }

        void InitializeBaseType()
        {
            baseType = appdomain.GetType(clrType.BaseType);
            if (baseType.TypeForCLR == typeof(Enum) || baseType.TypeForCLR == typeof(object) || baseType.TypeForCLR == typeof(ValueType) || baseType.TypeForCLR == typeof(System.Enum))
            {//都是这样，无所谓
                baseType = null;
            }
            isBaseTypeInitialized = true;
        }

        public FieldInfo GetField(int hash)
        {
            var dic = Fields;
            FieldInfo res;
            if (dic.TryGetValue(hash, out res))
                return res;
            else if (BaseType != null)
                return ((CLRType)BaseType).GetField(hash);
            else
                return null;
        }

        void InitializeMethods()
        {
            methods = new Dictionary<string, List<CLRMethod>>();
            constructors = new List<CLRMethod>();
            foreach (var i in clrType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (i.IsPrivate)
                    continue;
                List<CLRMethod> lst;
                if (!methods.TryGetValue(i.Name, out lst))
                {
                    lst = new List<CLRMethod>();
                    methods[i.Name] = lst;
                }
                lst.Add(new CLRMethod(i, this, appdomain));
            }
            foreach (var i in clrType.GetConstructors())
            {
                constructors.Add(new CLRMethod(i, this, appdomain));
            }
        }
        public List<IMethod> GetMethods()
        {
            if (methods == null)
                InitializeMethods();
            List<IMethod> res = new List<IMethod>();
            foreach (var i in methods)
            {
                foreach (var j in i.Value)
                    res.Add(j);
            }

            return res;
        }

        public IMethod GetVirtualMethod(IMethod method)
        {
            var m = GetMethod(method.Name, method.Parameters, null, method.ReturnType);
            if (m == null)
            {
                return method;
            }
            else
                return m;
        }

        void InitializeFields()
        {
            fieldMapping = new Dictionary<string, int>();
            fieldInfoCache = new Dictionary<int, FieldInfo>();

            var fields = clrType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var i in fields)
            {
                if (i.IsPublic || i.IsFamily)
                {
                    int hashCode = i.GetHashCode();
                    fieldMapping[i.Name] = hashCode;
                    fieldInfoCache[hashCode] = i;
                }
            }
        }
        public int GetFieldIndex(object token)
        {
            if (fieldMapping == null)
                InitializeFields();
            int idx;
            int hashCode = token.GetHashCode();
            if (fieldTokenMapping == null)
                fieldTokenMapping = new Dictionary<int, int>();
            if (fieldTokenMapping.TryGetValue(hashCode, out idx))
                return idx;
            FieldReference f = token as FieldReference;
            if (fieldMapping.TryGetValue(f.Name, out idx))
            {
                fieldTokenMapping[hashCode] = idx;
                return idx;
            }

            return -1;
        }
        public IType FindGenericArgument(string key)
        {
            if (genericArguments != null)
            {
                foreach (var i in genericArguments)
                {
                    if (i.Key == key)
                        return i.Value;
                }
            }
            return null;
        }
        public IMethod GetMethod(string name, int paramCount)
        {
            if (methods == null)
                InitializeMethods();
            List<CLRMethod> lst;
            if (methods.TryGetValue(name, out lst))
            {
                foreach (var i in lst)
                {
                    if (i.ParameterCount == paramCount)
                        return i;
                }
            }
            return null;
        }

        public IMethod GetMethod(string name, List<IType> param, IType[] genericArguments, IType returnType = null)
        {
            if (methods == null)
                InitializeMethods();
            List<CLRMethod> lst;
            IMethod genericMethod = null;
            if (methods.TryGetValue(name, out lst))
            {
                foreach (var i in lst)
                {
                    if (i.ParameterCount == param.Count)
                    {
                        bool match = true;
                        if (genericArguments != null && i.GenericParameterCount == genericArguments.Length)
                        {
                            for (int j = 0; j < param.Count; j++)
                            {
                                var p = i.Parameters[j].TypeForCLR;
                                var q = param[j].TypeForCLR;

                                if (i.Parameters[j].HasGenericParameter)
                                {
                                    //TODO should match the generic parameters;
                                    continue;
                                }
                                if (q != p)
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if (match)
                            {
                                genericMethod = i;
                                break;
                            }                            
                        }
                        else
                        {
                            match = genericArguments == null;
                            for (int j = 0; j < param.Count; j++)
                            {
                                var typeA = param[j].TypeForCLR.IsByRef ? param[j].TypeForCLR.GetElementType() : param[j].TypeForCLR;
                                var typeB = i.Parameters[j].TypeForCLR.IsByRef ? i.Parameters[j].TypeForCLR.GetElementType() : i.Parameters[j].TypeForCLR;

                                if (typeA != typeB)
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if (match)
                            {
                                match = returnType == null || i.ReturnType.TypeForCLR == returnType.TypeForCLR;
                            }
                            if (match)
                            {
                                
                                if (i.IsGenericInstance)
                                {
                                    if (i.GenericArguments.Length == genericArguments.Length)
                                    {
                                        for (int j = 0; j < genericArguments.Length; j++)
                                        {
                                            if(i.GenericArguments[j] != genericArguments[j])
                                            {
                                                match = false;
                                                break;
                                            }
                                        }
                                        if (match)
                                            return i;
                                    }
                                }
                                else
                                    return i;
                            }
                        }
                    }
                }
            }
            if (genericArguments != null && genericMethod != null)
            {
                var m = genericMethod.MakeGenericMethod(genericArguments);
                lst.Add((CLRMethod)m);
                return m;
            }
            return null;
        }
        public bool CanAssignTo(IType type)
        {
            if (this == type)
            {
                return true;
            }
            else
                return false;
        }

        public IMethod GetConstructor(List<IType> param)
        {
            if (constructors == null)
                InitializeMethods();
            foreach (var i in constructors)
            {
                if (i.ParameterCount == param.Count)
                {
                    bool match = true;

                    for (int j = 0; j < param.Count; j++)
                    {
                        if (param[j].TypeForCLR != i.Parameters[j].TypeForCLR)
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        return i;
                    }
                }
            }
            
            return null;
        }

        public IType MakeGenericInstance(KeyValuePair<string, IType>[] genericArguments)
        {
            if (genericInstances == null)
                genericInstances = new List<CLRType>();
            foreach (var i in genericInstances)
            {
                bool match = true;
                for (int j = 0; j < genericArguments.Length; j++)
                {
                    if (i.genericArguments[j].Value != genericArguments[j].Value)
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                    return i;
            }
            Type[] args = new Type[genericArguments.Length];
            for (int i = 0; i < genericArguments.Length; i++)
            {
                args[i] = genericArguments[i].Value.TypeForCLR;
            }
            Type newType = clrType.MakeGenericType(args);
            var res = new CLRType(newType, appdomain);
            res.genericArguments = genericArguments;

            genericInstances.Add(res);
            return res;
        }

        public IType MakeByRefType()
        {
            if (byRefType == null)
            {
                Type t = clrType.MakeByRefType();
                byRefType = new CLRType(t, appdomain);
            }
            return byRefType;
        }
        public IType MakeArrayType()
        {
            if (arrayType == null)
            {
                Type t = clrType.MakeArrayType();
                arrayType = new CLRType(t, appdomain);
            }
            return arrayType;
        }

        public IType ResolveGenericType(IType contextType)
        {
            throw new NotImplementedException();
        }
    }
}
