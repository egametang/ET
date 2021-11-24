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
    public class ILRuntimeWrapperType : Type
    {
        CLRType type;
        Type et;

        public CLRType CLRType { get { return type; } }

        public Type RealType { get { return et; } }

        public override Guid GUID
        {
            get
            {
                return et.GUID;
            }
        }

        public override Module Module
        {
            get
            {
                return et.Module;
            }
        }

        public override Assembly Assembly
        {
            get
            {
                return et.Assembly;
            }
        }

        public override string FullName
        {
            get
            {
                return et.FullName;
            }
        }

        public override string Namespace
        {
            get
            {
                return et.Namespace;
            }
        }

        public override string AssemblyQualifiedName
        {
            get
            {
                return et.AssemblyQualifiedName;
            }
        }

        public override Type BaseType
        {
            get
            {
                return et.BaseType;
            }
        }

        public override Type UnderlyingSystemType
        {
            get
            {
                return et.UnderlyingSystemType;
            }
        }

        public override string Name
        {
            get
            {
                return et.Name;
            }
        }

        public ILRuntimeWrapperType(CLRType t)
        {
            type = t;
            et = t.TypeForCLR;
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            return et.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            return et.GetConstructor(bindingAttr, binder, callConvention, types, modifiers);
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            return et.GetConstructors(bindingAttr);
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            return et.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            return et.GetMethods(bindingAttr);
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            return et.GetField(name, bindingAttr);
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            return et.GetFields(bindingAttr);
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            return et.GetInterface(name, ignoreCase);
        }

        public override Type[] GetInterfaces()
        {
            return et.GetInterfaces();
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            return et.GetEvent(name, bindingAttr);
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            return et.GetEvents(bindingAttr);
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            return et.GetProperty(name, bindingAttr);
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            return et.GetProperties(bindingAttr);
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            return et.GetNestedTypes(bindingAttr);
        }

        public override int GetHashCode()
        {
            return type.GetHashCode();
        }

        public override string ToString()
        {
            return type.FullName;
        }
        public override bool IsAssignableFrom(Type c)
        {
            if (c is ILRuntimeWrapperType)
                c = ((ILRuntimeWrapperType)c).RealType;
            if (c is ILRuntimeType)
                c = ((ILRuntimeType)c).ILType.TypeForCLR;
            return et.IsAssignableFrom(c);
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

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            return et.GetNestedType(name, bindingAttr);
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            return et.GetMembers(bindingAttr);
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            return et.Attributes;
        }

        protected override bool IsArrayImpl()
        {
            return et.IsArray;
        }

        protected override bool IsByRefImpl()
        {
            return et.IsByRef;
        }

        protected override bool IsPointerImpl()
        {
            return et.IsPointer;
        }

        protected override bool IsPrimitiveImpl()
        {
            return et.IsPrimitive;
        }

        protected override bool IsCOMObjectImpl()
        {
            return et.IsCOMObject;
        }

        public override bool IsGenericType
        {
            get { return et.IsGenericType; }
        }

        public override bool IsGenericTypeDefinition
        {
            get
            {
                return et.IsGenericTypeDefinition;
            }
        }

        public override Type GetGenericTypeDefinition()
        {
            return et.GetGenericTypeDefinition();
        }

        public override Type[] GetGenericArguments()
        {
            return et.GetGenericArguments();
        }

        public override bool IsGenericParameter
        {
            get
            {
                return et.IsGenericParameter;
            }
        }
        public override Type GetElementType()
        {
            return et.GetElementType();
        }

        protected override bool HasElementTypeImpl()
        {
            return et.HasElementType;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return et.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return et.GetCustomAttributes(attributeType, inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return et.IsDefined(attributeType, inherit);
        }
    }
}
