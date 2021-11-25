﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;

using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;

namespace ILRuntime.Reflection
{
    public class ILRuntimePropertyInfo : PropertyInfo
    {
        ILMethod getter, setter;
        ILType dType;
        Mono.Cecil.PropertyDefinition definition;
        ILRuntime.Runtime.Enviorment.AppDomain appdomain;

        Attribute[] customAttributes;
        Type[] attributeTypes;

        public ILMethod Getter
        {
            get { return getter; }
            set
            {
                getter = value;
            }
        }

        public ILMethod Setter
        {
            get { return setter; }
            set
            {
                setter = value;
            }
        }

        public bool IsPublic
        {
            get
            {
                if (getter != null)
                    return getter.Definition.IsPublic;
                else
                    return setter.Definition.IsPublic;
            }
        }

        public bool IsStatic
        {
            get
            {
                if (getter != null)
                    return getter.IsStatic;
                else
                    return setter.IsStatic;
            }
        }
        public ILRuntimePropertyInfo(Mono.Cecil.PropertyDefinition definition, ILType dType)
        {
            this.definition = definition;
            this.dType = dType;
            appdomain = dType.AppDomain;
        }

        void InitializeCustomAttribute()
        {
            customAttributes = new Attribute[definition.CustomAttributes.Count];
            attributeTypes = new Type[customAttributes.Length];
            for (int i = 0; i < definition.CustomAttributes.Count; i++)
            {
                var attribute = definition.CustomAttributes[i];
                var at = appdomain.GetType(attribute.AttributeType, null, null);
                try
                {
                    Attribute ins = attribute.CreateInstance(at, appdomain) as Attribute;

                    attributeTypes[i] = at.ReflectionType;
                    customAttributes[i] = ins;
                }
                catch
                {
                    attributeTypes[i] = typeof(Attribute);
                }
            }
        }

        public override string Name
        {
            get
            {
                return definition.Name;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return dType.ReflectionType;
            }
        }

        public override PropertyAttributes Attributes
        {
            get
            {
                return PropertyAttributes.None;
            }
        }

        public override bool CanRead
        {
            get
            {
                return getter != null;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return setter != null;
            }
        }

        public override Type PropertyType
        {
            get
            {
                if (getter != null)
                    return getter.ReturnType.ReflectionType;
                else
                {
                    return setter.Parameters[0].ReflectionType;
                }
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return dType.ReflectionType;
            }
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
            for (int i = 0; i < customAttributes.Length; i++)
            {
                if (attributeTypes[i].Equals(attributeType))
                {
                    res.Add(customAttributes[i]);
                }
            }
            return res.ToArray();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (customAttributes == null)
                InitializeCustomAttribute();

            for (int i = 0; i < customAttributes.Length; i++)
            {
                if (attributeTypes[i].Equals(attributeType))
                {
                    return true;
                }
            }
            return false;
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            if (getter != null)
                return getter.ReflectionMethodInfo;
            return null;
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            return new ParameterInfo[0];
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            if (setter != null)
                return setter.ReflectionMethodInfo;
            return null;
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            var indexCnt = index != null ? index.Length : 0;
            if (getter.ParameterCount <= indexCnt)
            {
                using (var ctx = appdomain.BeginInvoke(getter))
                {
                    if (!IsStatic)
                        ctx.PushObject(obj);
                    for (int i = 0; i < getter.ParameterCount; i++)
                    {
                        ctx.PushObject(index[i], !getter.Parameters[i].IsValueType);
                    }
                    ctx.Invoke();
                    return ctx.ReadObject(getter.ReturnType.TypeForCLR);
                }
            }
            else
                throw new ArgumentException("Index count mismatch");
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            var indexCnt = index != null ? index.Length : 0;
            if (setter.ParameterCount <= indexCnt + 1)
            {
                using (var ctx = appdomain.BeginInvoke(setter))
                {
                    if (!IsStatic)
                        ctx.PushObject(obj);
                    for (int i = 0; i < setter.ParameterCount - 1; i++)
                    {
                        ctx.PushObject(index[i], !setter.Parameters[i].IsValueType);
                    }
                    ctx.PushObject(value, !setter.Parameters[setter.ParameterCount - 1].IsValueType);
                    ctx.Invoke();
                }
            }
            else
                throw new ArgumentException("Index count mismatch");
        }
    }
}
