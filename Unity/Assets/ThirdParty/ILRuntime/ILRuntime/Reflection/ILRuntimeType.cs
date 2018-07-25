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
        object[] customAttributes;
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
            customAttributes = new object[type.TypeDefinition.CustomAttributes.Count];
            attributeTypes = new Type[customAttributes.Length];
            for (int i = 0; i < type.TypeDefinition.CustomAttributes.Count; i++)
            {
                var attribute = type.TypeDefinition.CustomAttributes[i];
                var at = appdomain.GetType(attribute.AttributeType, type, null);
                try
                {
                    object ins = attribute.CreateInstance(at, appdomain);

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
            int cnt = type.TypeDefinition.HasProperties ? type.TypeDefinition.Properties.Count : 0;
            properties = new ILRuntimePropertyInfo[cnt];
            for (int i = 0; i < cnt; i++)
            {
                Mono.Cecil.PropertyDefinition pd = type.TypeDefinition.Properties[i];
                ILRuntimePropertyInfo pi = new ILRuntimePropertyInfo(pd, type);
                properties[i] = pi;
                if (pd.GetMethod != null)
                    pi.Getter = type.GetMethod(pd.GetMethod.Name, 0) as ILMethod;
                if (pd.SetMethod != null)
                    pi.Setter = type.GetMethod(pd.SetMethod.Name, 1) as ILMethod;
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

            return customAttributes;
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (customAttributes == null)
                InitializeCustomAttribute();
            List<object> res = new List<object>();
            for(int i = 0; i < customAttributes.Length; i++)
            {
                if (attributeTypes[i].Equals(attributeType))
                    res.Add(customAttributes[i]);
            }
            return res.ToArray();
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
            if (type.FirstCLRInterface != null)
                return new Type[] { type.FirstCLRInterface.TypeForCLR };
            else
                return new Type[0];
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
            return methods;
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

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            IMethod res;
            if (types == null)
                res = type.GetMethod(name);
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
                bool declearedOnly = (bindingAttr & BindingFlags.DeclaredOnly) == BindingFlags.DeclaredOnly;
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
        public override int GetHashCode()
        {
            return type.GetHashCode();
        }
        public override bool Equals(object o)
        {
            return o is ILRuntimeType ? ((ILRuntimeType)o).type == type : false;
        }
        public override bool IsGenericType
        {
            get
            {
                return type.HasGenericParameter;
            }
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
