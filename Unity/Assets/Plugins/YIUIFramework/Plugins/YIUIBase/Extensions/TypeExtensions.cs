using System;
using System.Collections.Generic;
using System.Reflection;

namespace YIUIFramework
{
    /// <summary>
    /// <see cref="System.Type"/>.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Check whether the type if a struct
        /// </summary>
        public static bool IsStruct(this Type type)
        {
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum;
        }

        /// <summary>
        /// Gets <see cref="FieldInfo"/> including base classes.
        /// </summary>
        public static FieldInfo[] GetFieldInfosIncludingBaseClasses(
            this Type type, BindingFlags bindingFlags)
        {
            var fieldInfos = type.GetFields(bindingFlags);
            if (type.BaseType == typeof(object))
            {
                return fieldInfos;
            }

            var fieldInfoList = new List<FieldInfo>(fieldInfos);
            while (type.BaseType != typeof(object))
            {
                type       = type.BaseType;
                fieldInfos = type.GetFields(bindingFlags);

                // Look for fields we do not have listed yet and merge them
                // into the main list
                foreach (var fieldInfo in fieldInfos)
                {
                    bool found = false;
                    foreach (var recordField in fieldInfoList)
                    {
                        if (recordField.Name == fieldInfo.Name &&
                            recordField.DeclaringType == fieldInfo.DeclaringType)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        fieldInfoList.Add(fieldInfo);
                    }
                }
            }

            return fieldInfoList.ToArray();
        }
    }
}