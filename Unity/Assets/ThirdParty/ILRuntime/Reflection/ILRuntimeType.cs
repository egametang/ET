using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntime.Reflection
{
    public class ILRuntimeType : Type
    {
        ILType type;
        Runtime.Enviorment.AppDomain appdomain;
        Attribute[] customAttributes;
        Type[] attributeTypes;
        ILRuntimeFieldInfo[] fields;
        ILRuntimePropertyInfo[] properties;
        ILRuntimeMethodInfo[] methods;

        public ILType ILType { get { return type; } }

        public ILRuntimeType(ILType t)
        {
            type = t;
            appdomain = t.AppDomain;
        }

        void InitializeCustomAttribute()
        {
            if(type.TypeDefinition == null)
            {
                customAttributes = new Attribute[0];
                attributeTypes = new Type[0];
                return;
            }
            customAttributes = new Attribute[type.TypeDefinition.CustomAttributes.Count];
            attributeTypes = new Type[customAttributes.Length];
            for (int i = 0; i < type.TypeDefinition.CustomAttributes.Count; i++)
            {
                var attribute = type.TypeDefinition.CustomAttributes[i];
                var at = appdomain.GetType(attribute.AttributeType, type, null);
                try
                {
                    Attribute ins = attribute.CreateInstance(at, appdomain) as Attribute;

                    attributeTypes[i] = at.ReflectionType is ILRuntimeWrapperType ? at.TypeForCLR : at.ReflectionType;
                    customAttributes[i] = ins;
                }
                catch
                {
                    attributeTypes[i] = typeof(Attribute);
                }
            }

        }

        void InitializeProperties()
        {
            if (type.TypeDefinition == null)
            {
                properties = new ILRuntimePropertyInfo[0];
                return;
            }
            int cnt = type.TypeDefinition.HasProperties ? type.TypeDefinition.Properties.Count : 0;
            properties = new ILRuntimePropertyInfo[cnt];
            for (int i = 0; i < cnt; i++)
            {
                Mono.Cecil.PropertyDefinition pd = type.TypeDefinition.Properties[i];
                ILRuntimePropertyInfo pi = new ILRuntimePropertyInfo(pd, type);
                properties[i] = pi;
                if (pd.GetMethod != null)
                    pi.Getter = type.GetMethod(pd.GetMethod.Name, pd.GetMethod.Parameters.Count) as ILMethod;
                if (pd.SetMethod != null)
                    pi.Setter = type.GetMethod(pd.SetMethod.Name, pd.SetMethod.Parameters.Count) as ILMethod;
            }
        }

        void InitializeMethods()
        {
            var methods = type.GetMethods();
            this.methods = new ILRuntimeMethodInfo[methods.Count];
            for(int i = 0; i < methods.Count; i++)
            {
                this.methods[i] = (ILRuntimeMethodInfo)((ILMethod)methods[i]).ReflectionMethodInfo;
            }
        }

        public override Type MakeGenericType(params Type[] typeArguments)
        {
            if (ILType.TypeReference.HasGenericParameters)
            {
                KeyValuePair<string, IType>[] ga = new KeyValuePair<string, IType>[typeArguments.Length];
                for (int i = 0; i < ga.Length; i++)
                {
                    string key = ILType.TypeReference.GenericParameters[i].Name;
                    if (typeArguments[i] is ILRuntimeType)
                        ga[i] = new KeyValuePair<string, IType>(key, ((ILRuntimeType)typeArguments[i]).ILType);
                    else
                        ga[i] = new KeyValuePair<string, IType>(key, ILType.AppDomain.GetType(typeArguments[i]));
                }
                return ILType.MakeGenericInstance(ga).ReflectionType;
            }
            else
                throw new NotSupportedException();
        }

        void InitializeFields()
        {
            int staticCnt = type.StaticFieldTypes != null ? type.StaticFieldTypes.Length : 0;
            fields = new ILRuntimeFieldInfo[type.TotalFieldCount + staticCnt];
            for (int i = 0; i < type.TotalFieldCount; i++)
            {
                Mono.Cecil.FieldDefinition fd;
                var t = type.GetField(i, out fd);
                fields[i] = new ILRuntimeFieldInfo(fd, this, i, t);
            }
            for (int i = type.TotalFieldCount; i < type.TotalFieldCount + staticCnt; i++)
            {
                fields[i] = new ILRuntimeFieldInfo(type.StaticFieldDefinitions[i - type.TotalFieldCount], this, true, i - type.TotalFieldCount);
            }
        }

        public override Assembly Assembly
        {
            get
            {
                return typeof(ILRuntimeType).Assembly;
            }
        }

        public override string AssemblyQualifiedName
        {
            get
            {
                return type.FullName;
            }
        }

        public override Type BaseType
        {
            get
            {
                if (type.IsEnum)
                    return typeof(Enum);
                else if (type.IsArray)
                    return typeof(Array);
                else
                {
                    return type.BaseType != null ? type.BaseType.ReflectionType : null;
                }
            }
        }

        public override string FullName
        {
            get
            {
                return type.FullName;
            }
        }

        public override Guid GUID
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Module Module
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string Name
        {
            get
            {
                return type.Name;
            }
        }

        public override string Namespace
        {
            get
            {
                return type.TypeDefinition.Namespace;
            }
        }

        public override Type UnderlyingSystemType
        {
            get
            {
                return typeof(ILTypeInstance);
            }
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            var ctors = type.GetConstructors();
            ConstructorInfo[] res = new ConstructorInfo[ctors.Count];
            for(int i = 0; i < res.Length; i++)
            {
                res[i] = ctors[i].ReflectionConstructorInfo;
            }
            return res;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            if (customAttributes == null)
                InitializeCustomAttribute();
            if (inherit && BaseType != null)
            {
                List<object> result = new List<object>();
                result.AddRange(customAttributes);
                result.AddRange(BaseType.GetCustomAttributes(inherit));
                return result.ToArray();
            }
            return customAttributes;
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (customAttributes == null)
                InitializeCustomAttribute();
            List<object> res = new List<object>();
            for (int i = 0; i < customAttributes.Length; i++)
            {
                if (attributeTypes[i].Equals((object)attributeType))
                    res.Add(customAttributes[i]);
            }
            if (inherit && BaseType != null)
            {
                res.AddRange(BaseType.GetCustomAttributes(attributeType, inherit));
            }
            return res.ToArray();
        }

        public override bool IsAssignableFrom(Type c)
        {
            IType type;
            if (c is ILRuntimeWrapperType)
            {
                type = ((ILRuntimeWrapperType)c).CLRType;
            }
            else if (c is ILRuntimeType)
            {
                type = ((ILRuntimeType)c).ILType;
            }
            else
                type = ILType.AppDomain.GetType(c);
            return type.CanAssignTo(ILType);
        }

        public override bool IsInstanceOfType(object o)
        {
            if (o == null)
            {
                return false;
            }

            var instance = o as ILTypeInstance;
            return IsAssignableFrom(instance != null ? instance.Type.ReflectionType : o.GetType());
        }

        public override Type GetElementType()
        {
            if (type.IsArray)
            {
                return type.ElementType.ReflectionType;
            }
            else
                throw new NotImplementedException();
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            if (fields == null)
                InitializeFields();
            foreach(var i in fields)
            {
                if (i.Name == name)
                    return i;
            }
            if (BaseType != null && BaseType is ILRuntimeWrapperType)
            {
                return BaseType.GetField(name, bindingAttr);
            }
            return null;
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            if (fields == null)
                InitializeFields();
            bool isPublic = (bindingAttr & BindingFlags.Public) == BindingFlags.Public;
            bool isPrivate = (bindingAttr & BindingFlags.NonPublic) == BindingFlags.NonPublic;
            bool isStatic = (bindingAttr & BindingFlags.Static) == BindingFlags.Static;
            bool isInstance = (bindingAttr & BindingFlags.Instance) == BindingFlags.Instance;
            List<FieldInfo> res = new List<FieldInfo>();
            foreach(var i in fields)
            {
                if (isPublic != i.IsPublic && isPrivate != !i.IsPublic)
                    continue;
                if ((isStatic != i.IsStatic) && (isInstance != !i.IsStatic))
                    continue;
                res.Add(i);
            }
            if ((bindingAttr & BindingFlags.DeclaredOnly) != BindingFlags.DeclaredOnly)
            {
                if (BaseType != null && (BaseType is ILRuntimeWrapperType))
                {
                    res.AddRange(BaseType.GetFields(bindingAttr));
                }
            }
            return res.ToArray();
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            if (type.FirstCLRInterface != null)
            {
                if (type.FirstCLRInterface.Name == name)
                    return type.FirstCLRInterface.TypeForCLR;
                else
                    return null;
            }
            else
                return null;
        }

        public override Type[] GetInterfaces()
        {
            if (type.Implements == null)
                return new Type[0];
            var interfaces = new Type[type.Implements.Length];
            for (int i = 0, length = type.Implements.Length; i < length; i++)
            {
                var t = type.Implements[i];
                if (t != null)
                    interfaces[i] = t.ReflectionType;
            }
            return interfaces;
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            if (methods == null)
                InitializeMethods();
            if (fields == null)
                InitializeFields();
            if (properties == null)
                InitializeProperties();
            MemberInfo[] res = new MemberInfo[methods.Length + fields.Length + properties.Length];
            for (int i = 0; i < methods.Length; i++)
            {
                res[i] = methods[i];
            }
            for (int i = methods.Length; i < methods.Length + fields.Length; i++)
            {
                res[i] = fields[i - methods.Length];
            }
            for (int i = methods.Length + fields.Length; i < res.Length; i++)
            {
                res[i] = properties[i - methods.Length - fields.Length];
            }

            return res;
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            if (methods == null)
                InitializeMethods();
            bool isPublic = (bindingAttr & BindingFlags.Public) == BindingFlags.Public;
            bool isPrivate = (bindingAttr & BindingFlags.NonPublic) == BindingFlags.NonPublic;
            bool isStatic = (bindingAttr & BindingFlags.Static) == BindingFlags.Static;
            bool isInstance = (bindingAttr & BindingFlags.Instance) == BindingFlags.Instance;
            List<MethodInfo> res = new List<MethodInfo>();
            foreach (var i in methods)
            {
                if (isPublic != i.IsPublic && isPrivate != !i.IsPublic)
                    continue;
                if ((isStatic != i.IsStatic) && (isInstance != !i.IsStatic))
                    continue;
                res.Add(i);
            }
            if ((bindingAttr & BindingFlags.DeclaredOnly) != BindingFlags.DeclaredOnly)
            {
                if (BaseType != null && (BaseType is ILRuntimeWrapperType || BaseType is ILRuntimeType))
                {
                    res.AddRange(BaseType.GetMethods(bindingAttr));
                }
            }
            return res.ToArray();
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            if (properties == null)
                InitializeProperties();
            bool isPublic = (bindingAttr & BindingFlags.Public) == BindingFlags.Public;
            bool isPrivate = (bindingAttr & BindingFlags.NonPublic) == BindingFlags.NonPublic;
            bool isStatic = (bindingAttr & BindingFlags.Static) == BindingFlags.Static;
            bool isInstance = (bindingAttr & BindingFlags.Instance) == BindingFlags.Instance;
            List<PropertyInfo> res = new List<PropertyInfo>();
            foreach (var i in properties)
            {
                if (isPublic != i.IsPublic && isPrivate != !i.IsPublic)
                    continue;
                if ((isStatic != i.IsStatic) && (isInstance != !i.IsStatic))
                    continue;
                res.Add(i);
            }
            if ((bindingAttr & BindingFlags.DeclaredOnly) != BindingFlags.DeclaredOnly)
            {
                if (BaseType != null && (BaseType is ILRuntimeWrapperType || BaseType is ILRuntimeType ))
                {
                    res.AddRange(BaseType.GetProperties(bindingAttr));
                }
            }
            return res.ToArray();
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (customAttributes == null)
                InitializeCustomAttribute();
            for (int i = 0; i < customAttributes.Length; i++)
            {
                if (attributeTypes[i].Equals(attributeType))
                    return true;
            }
            return false;
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            TypeAttributes res = TypeAttributes.Public;
            if (type.TypeDefinition == null)
            {
                return TypeAttributes.Class;
            }
            if (type.TypeDefinition.IsAbstract)
                res |= TypeAttributes.Abstract;
            if (!type.IsValueType)
                res |= TypeAttributes.Class;
            if (type.TypeDefinition.IsSealed)
                res |= TypeAttributes.Sealed;
            return res;
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            List<IType> param = new List<IType>();
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] is ILRuntimeType)
                    param.Add(((ILRuntimeType)types[i]).type);
                else
                {
                    var t = appdomain.GetType(types[i]);
                    if (t == null)
                        t = appdomain.GetType(types[i].AssemblyQualifiedName);
                    if (t == null)
                        throw new TypeLoadException();
                    param.Add(t);
                }
            }

            var res = type.GetConstructor(param);

            if (res != null)
                return ((ILMethod)res).ReflectionConstructorInfo;
            else
                return null;
        }

        public override Type[] GetGenericArguments()
        {
            var args = type.GenericArguments;
            Type[] res = new Type[args.Length];
            for(int i = 0; i < res.Length; i++)
            {
                res[i] = args[i].Value.ReflectionType;
            }
            return res;
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            IMethod res;
            bool declearedOnly = (bindingAttr & BindingFlags.DeclaredOnly) == BindingFlags.DeclaredOnly;

            if (types == null)
            {
                res = type.GetMethod(name);
                if (res == null && !declearedOnly && type.BaseType is ILType)
                    return BaseType.GetMethod(name, bindingAttr);
            }
            else
            {
                List<IType> param = new List<IType>();
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i] is ILRuntimeType)
                        param.Add(((ILRuntimeType)types[i]).type);
                    else
                    {
                        var t = appdomain.GetType(types[i]);
                        if (t == null)
                            t = appdomain.GetType(types[i].AssemblyQualifiedName);
                        if (t == null)
                            throw new TypeLoadException();
                        param.Add(t);
                    }
                }
                res = type.GetMethod(name, param, null, null, declearedOnly);
            }
            if (res != null)
                return ((ILMethod)res).ReflectionMethodInfo;
            else
                return null;
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            if (properties == null)
                InitializeProperties();

            foreach(var i in properties)
            {
                if (i.Name == name)
                    return i;
            }
            if ((bindingAttr & BindingFlags.DeclaredOnly) != BindingFlags.DeclaredOnly)
            {
                if (BaseType != null && BaseType is ILRuntimeWrapperType)
                {
                    return BaseType.GetProperty(name, bindingAttr);
                }
            }
            return null;
        }

        protected override bool HasElementTypeImpl()
        {
            return false;
        }
        protected override bool IsArrayImpl()
        {
            return type.IsArray;
        }

        protected override bool IsByRefImpl()
        {
            return false;
        }

        protected override bool IsCOMObjectImpl()
        {
            return false;
        }

        protected override bool IsPointerImpl()
        {
            return false;
        }

        protected override bool IsPrimitiveImpl()
        {
            return false;
        }

        public override string ToString()
        {
            return type.FullName;
        }
        public override int GetHashCode()
        {
            return type.GetHashCode();
        }
#if NET_4_6 || NET_STANDARD_2_0
        public override bool Equals(Type o)
        {
            return o is ILRuntimeType ? ((ILRuntimeType)o).type == type : false;
        }
#endif
        public override bool Equals(object o)
        {
            return o is ILRuntimeType ? ((ILRuntimeType)o).type == type : false;
        }
        public override bool IsGenericType
        {
            get
            {
                return type.HasGenericParameter || type.GenericArguments != null;
            }
        }

        public override Type GetGenericTypeDefinition()
        {
            var def = type.GetGenericDefinition();

            return def != null ? def.ReflectionType : null;
        }

        public override bool IsGenericTypeDefinition
        {
            get
            {
                return type.HasGenericParameter;
            }
        }
    }
}
