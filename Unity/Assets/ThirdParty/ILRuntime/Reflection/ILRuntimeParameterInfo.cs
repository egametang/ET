using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;

using ILRuntime.CLR.TypeSystem;

namespace ILRuntime.Reflection
{
    public class ILRuntimeParameterInfo : ParameterInfo
    {
        public IType IType { get; private set; }
        public ILRuntime.Runtime.Enviorment.AppDomain AppDomain{ get; private set; }

        Mono.Cecil.ParameterDefinition definition;
        Attribute[] customAttributes;
        Type[] attributeTypes;

        public ILRuntimeParameterInfo(Mono.Cecil.ParameterDefinition definition, IType type, MemberInfo member, ILRuntime.Runtime.Enviorment.AppDomain appdomain)
        {
            this.IType = type;
            this.definition = definition;
            this.AppDomain = appdomain;

            AttrsImpl = (ParameterAttributes)definition.Attributes;
            ClassImpl = type.ReflectionType;
            DefaultValueImpl = definition.Constant;
            MemberImpl = member;
            NameImpl = definition.Name;
            PositionImpl = definition.Index;
        }

        void InitializeCustomAttribute()
        {
            customAttributes = new Attribute[definition.CustomAttributes.Count];
            attributeTypes = new Type[customAttributes.Length];
            for (int i = 0; i < definition.CustomAttributes.Count; i++)
            {
                var attribute = definition.CustomAttributes[i];
                var at = AppDomain.GetType(attribute.AttributeType, null, null);
                try
                {
                    Attribute ins = attribute.CreateInstance(at, AppDomain) as Attribute;

                    attributeTypes[i] = at.ReflectionType;
                    customAttributes[i] = ins;
                }
                catch
                {
                    attributeTypes[i] = typeof(Attribute);
                }
            }
        }

        public override bool HasDefaultValue
        {
            get { return definition.HasDefault; }
        }

        public override object DefaultValue
        {
            get { return DefaultValueImpl; }
        }

        public override object RawDefaultValue
        {
            get { return DefaultValueImpl; }
        }

        public override int MetadataToken
        {
            get { return definition.MetadataToken.ToInt32(); }
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
            List<Attribute> res = new List<Attribute>();
            for (int i = 0; i < customAttributes.Length; i++)
            {
                if (attributeTypes[i].Equals(attributeType))
                    res.Add(customAttributes[i]);
            }
            return res.ToArray();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            var result = GetCustomAttributes(attributeType, inherit);
            return result != null && result.Length > 0;
        }

        public override string ToString()
        {
            return definition == null ? base.ToString() : definition.ToString();
        }
    }
}