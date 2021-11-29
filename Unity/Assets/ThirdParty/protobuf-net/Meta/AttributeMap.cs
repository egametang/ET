#if !NO_RUNTIME
using System;
using System.Linq;
#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
using IKVM.Reflection;
#else
using System.Reflection;
#endif

namespace ProtoBuf.Meta
{
    internal abstract class AttributeMap
    {
#if DEBUG
        [Obsolete("Please use AttributeType instead")]
        new public Type GetType() { return AttributeType; }
#endif
        public override string ToString() => AttributeType?.FullName ?? "";
        public abstract bool TryGet(string key, bool publicOnly, out object value);
        public bool TryGet(string key, out object value)
        {
            return TryGet(key, true, out value);
        }
        public abstract Type AttributeType { get; }
        public static AttributeMap[] Create(TypeModel model, Type type, bool inherit)
        {
#if FEAT_IKVM
            Type attribType = model.MapType(typeof(System.Attribute));
            System.Collections.Generic.IList<CustomAttributeData> all = type.__GetCustomAttributes(attribType, inherit);
            AttributeMap[] result = new AttributeMap[all.Count];
            int index = 0;
            foreach (CustomAttributeData attrib in all)
            {
                result[index++] = new AttributeDataMap(attrib);
            }
            return result;
#else
#if WINRT || COREFX
            Attribute[] all = System.Linq.Enumerable.ToArray(type.GetTypeInfo().GetCustomAttributes(inherit));
#else
            //object[] all = type.GetCustomAttributes(inherit);
            // 过滤掉null的Attribute
            object[] all = type.GetCustomAttributes(inherit).Where((x)=>x != null).ToArray();
#endif
            AttributeMap[] result = new AttributeMap[all.Length];
            for(int i = 0 ; i < all.Length ; i++)
            {
                result[i] = new ReflectionAttributeMap((Attribute)all[i]);
            }
            return result;
#endif
        }

        public static AttributeMap[] Create(TypeModel model, MemberInfo member, bool inherit)
        {
#if FEAT_IKVM
            System.Collections.Generic.IList<CustomAttributeData> all = member.__GetCustomAttributes(model.MapType(typeof(Attribute)), inherit);
            AttributeMap[] result = new AttributeMap[all.Count];
            int index = 0;
            foreach (CustomAttributeData attrib in all)
            {
                result[index++] = new AttributeDataMap(attrib);
            }
            return result;
#else
#if WINRT || COREFX
            Attribute[] all = System.Linq.Enumerable.ToArray(member.GetCustomAttributes(inherit));
#else
            //object[] all = member.GetCustomAttributes(inherit);
            // 过滤掉null的Attribute
            object[] all = member.GetCustomAttributes(inherit).Where((x)=>x != null).ToArray();
#endif
            AttributeMap[] result = new AttributeMap[all.Length];
            for(int i = 0 ; i < all.Length ; i++)
            {
                result[i] = new ReflectionAttributeMap((Attribute)all[i]);
            }
            return result;
#endif
        }
        public static AttributeMap[] Create(TypeModel model, Assembly assembly)
        {

#if FEAT_IKVM
            const bool inherit = false;
            System.Collections.Generic.IList<CustomAttributeData> all = assembly.__GetCustomAttributes(model.MapType(typeof(Attribute)), inherit);
            AttributeMap[] result = new AttributeMap[all.Count];
            int index = 0;
            foreach (CustomAttributeData attrib in all)
            {
                result[index++] = new AttributeDataMap(attrib);
            }
            return result;
#else
#if WINRT || COREFX
            Attribute[] all = System.Linq.Enumerable.ToArray(assembly.GetCustomAttributes());
#else
            const bool inherit = false;
            //object[] all = assembly.GetCustomAttributes(inherit);
            // 过滤掉null的Attribute
            object[] all = assembly.GetCustomAttributes(inherit).Where((x)=>x != null).ToArray();
#endif
            AttributeMap[] result = new AttributeMap[all.Length];
            for(int i = 0 ; i < all.Length ; i++)
            {
                result[i] = new ReflectionAttributeMap((Attribute)all[i]);
            }
            return result;
#endif
        }
#if FEAT_IKVM
        private sealed class AttributeDataMap : AttributeMap
        {
            public override Type AttributeType
            {
                get { return attribute.Constructor.DeclaringType; }
            }
            private readonly CustomAttributeData attribute;
            public AttributeDataMap(CustomAttributeData attribute)
            {
                this.attribute = attribute;
            }
            public override bool TryGet(string key, bool publicOnly, out object value)
            {
                foreach (CustomAttributeNamedArgument arg in attribute.NamedArguments)
                {
                    if (string.Equals(arg.MemberInfo.Name, key, StringComparison.OrdinalIgnoreCase))
                    {
                        value = arg.TypedValue.Value;
                        return true;
                    }
                }

                    
                int index = 0;
                ParameterInfo[] parameters = attribute.Constructor.GetParameters();
                foreach (CustomAttributeTypedArgument arg in attribute.ConstructorArguments)
                {
                    if (string.Equals(parameters[index++].Name, key, StringComparison.OrdinalIgnoreCase))
                    {
                        value = arg.Value;
                        return true;
                    }
                }
                value = null;
                return false;
            }
        }
#else
        public abstract object Target { get; }
        private sealed class ReflectionAttributeMap : AttributeMap
        {
            public override object Target
            {
                get { return attribute; }
            }
            public override Type AttributeType
            {
                get { return attribute.GetType(); }
            }
            public override bool TryGet(string key, bool publicOnly, out object value)
            {
                MemberInfo[] members = Helpers.GetInstanceFieldsAndProperties(attribute.GetType(), publicOnly);
                foreach (MemberInfo member in members)
                {
#if FX11
                    if (member.Name.ToUpper() == key.ToUpper())
#else
                    if (string.Equals(member.Name, key, StringComparison.OrdinalIgnoreCase))
#endif
                    {
                        PropertyInfo prop = member as PropertyInfo;
                        if (prop != null) {
                            //value = prop.GetValue(attribute, null);
                            value = prop.GetGetMethod(true).Invoke(attribute, null);
                            return true;
                        }
                        FieldInfo field = member as FieldInfo;
                        if (field != null) {
                            value = field.GetValue(attribute);
                            return true;
                        }

                        throw new NotSupportedException(member.GetType().Name);
                    }
                }
                value = null;
                return false;
            }
            private readonly Attribute attribute;
            public ReflectionAttributeMap(Attribute attribute)
            {
                this.attribute = attribute;
            }
        }
#endif
    }
}
#endif