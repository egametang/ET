/* Copyright 2015-2017 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;

namespace MongoDB.Driver.Support
{
    internal static class ReflectionExtensions
    {
        public static object GetDefaultValue(this Type type)
        {
            if (type.GetTypeInfo().IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }

        public static bool ImplementsInterface(this Type type, Type iface)
        {
            if (type.Equals(iface))
            {
                return true;
            }

            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType && type.GetGenericTypeDefinition().Equals(iface))
            {
                return true;
            }

            return typeInfo.GetInterfaces().Any(i => i.ImplementsInterface(iface));
        }

        public static bool IsNullable(this Type type)
        {
            return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsNullableEnum(this Type type)
        {
            if (!IsNullable(type))
            {
                return false;
            }

            return GetNullableUnderlyingType(type).GetTypeInfo().IsEnum;
        }

        public static bool IsNumeric(this Type type)
        {
            return
                type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(double) ||
                type == typeof(decimal) ||
                type == typeof(Decimal128);
        }

        public static bool IsConvertibleToEnum(this Type type)
        {
            return
                type == typeof(sbyte) ||
                type == typeof(short) ||
                type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(byte) ||
                type == typeof(ushort) ||
                type == typeof(uint) ||
                type == typeof(ulong) ||
                type == typeof(Enum) ||
                type == typeof(string);
        }

        public static Type GetNullableUnderlyingType(this Type type)
        {
            if (!IsNullable(type))
            {
                throw new ArgumentException("Type must be nullable.", "type");
            }

            return type.GetTypeInfo().GetGenericArguments()[0];
        }

        public static Type GetSequenceElementType(this Type type)
        {
            Type ienum = FindIEnumerable(type);
            if (ienum == null) { return type; }
            return ienum.GetTypeInfo().GetGenericArguments()[0];
        }

        public static Type FindIEnumerable(this Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
            {
                return null;
            }

            var seqTypeInfo = seqType.GetTypeInfo();
            if (seqTypeInfo.IsGenericType && seqTypeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return seqType;
            }

            if (seqTypeInfo.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            }

            if (seqTypeInfo.IsGenericType)
            {
                foreach (Type arg in seqType.GetTypeInfo().GetGenericArguments())
                {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.GetTypeInfo().IsAssignableFrom(seqType))
                    {
                        return ienum;
                    }
                }
            }

            Type[] ifaces = seqTypeInfo.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    Type ienum = FindIEnumerable(iface);
                    if (ienum != null) { return ienum; }
                }
            }

            if (seqTypeInfo.BaseType != null && seqTypeInfo.BaseType != typeof(object))
            {
                return FindIEnumerable(seqTypeInfo.BaseType);
            }

            return null;
        }
    }
}
