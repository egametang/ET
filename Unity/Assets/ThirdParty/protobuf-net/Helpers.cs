
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
#if COREFX
using System.Linq;
#endif
#if PROFILE259
using System.Reflection;
using System.Linq;
#else
using System.Reflection;
#endif

namespace ProtoBuf
{
    /// <summary>
    /// Not all frameworks are created equal (fx1.1 vs fx2.0,
    /// micro-framework, compact-framework,
    /// silverlight, etc). This class simply wraps up a few things that would
    /// otherwise make the real code unnecessarily messy, providing fallback
    /// implementations if necessary.
    /// </summary>
    internal sealed class Helpers
    {
        private Helpers() { }

        public static StringBuilder AppendLine(StringBuilder builder)
        {
            return builder.AppendLine();
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugWriteLine(string message, object obj)
        {
#if DEBUG
            string suffix;
            try
            {
                suffix = obj == null ? "(null)" : obj.ToString();
            }
            catch
            {
                suffix = "(exception)";
            }
            DebugWriteLine(message + ": " + suffix);
#endif
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugWriteLine(string message)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(message);
#endif
        }
        [System.Diagnostics.Conditional("TRACE")]
        public static void TraceWriteLine(string message)
        {
#if TRACE
#if CF2 || PORTABLE || COREFX || PROFILE259
			System.Diagnostics.Debug.WriteLine(message);
#else
            System.Diagnostics.Trace.WriteLine(message);
#endif
#endif
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugAssert(bool condition, string message)
        {
#if DEBUG
            if (!condition)
            {
                System.Diagnostics.Debug.Assert(false, message);
            }
#endif
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugAssert(bool condition, string message, params object[] args)
        {
#if DEBUG
            if (!condition) DebugAssert(false, string.Format(message, args));
#endif
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugAssert(bool condition)
        {
#if DEBUG   
            if (!condition && System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
            System.Diagnostics.Debug.Assert(condition);
#endif
        }
#if !NO_RUNTIME
        public static void Sort(int[] keys, object[] values)
        {
            // bubble-sort; it'll work on MF, has small code,
            // and works well-enough for our sizes. This approach
            // also allows us to do `int` compares without having
            // to go via IComparable etc, so win:win
            bool swapped;
            do
            {
                swapped = false;
                for (int i = 1; i < keys.Length; i++)
                {
                    if (keys[i - 1] > keys[i])
                    {
                        int tmpKey = keys[i];
                        keys[i] = keys[i - 1];
                        keys[i - 1] = tmpKey;
                        object tmpValue = values[i];
                        values[i] = values[i - 1];
                        values[i - 1] = tmpValue;
                        swapped = true;
                    }
                }
            } while (swapped);
        }
#endif

#if COREFX
		internal static MemberInfo GetInstanceMember(TypeInfo declaringType, string name)
        {
            var members = declaringType.AsType().GetMember(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            switch(members.Length)
            {
                case 0: return null;
                case 1: return members[0];
                default: throw new AmbiguousMatchException(name);
            }
        }
        internal static MethodInfo GetInstanceMethod(Type declaringType, string name)
        {
            foreach (MethodInfo method in declaringType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (method.Name == name) return method;
            }
            return null;
        }
        internal static MethodInfo GetInstanceMethod(TypeInfo declaringType, string name)
        {
            return GetInstanceMethod(declaringType.AsType(), name); ;
        }
        internal static MethodInfo GetStaticMethod(Type declaringType, string name)
        {
            foreach (MethodInfo method in declaringType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (method.Name == name) return method;
            }
            return null;
        }

        internal static MethodInfo GetStaticMethod(TypeInfo declaringType, string name)
        {
            return GetStaticMethod(declaringType.AsType(), name);
        }
        internal static MethodInfo GetStaticMethod(Type declaringType, string name, Type[] parameterTypes)
        {
            foreach(MethodInfo method in declaringType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (method.Name == name && IsMatch(method.GetParameters(), parameterTypes)) return method;
            }
            return null;
        }
        internal static MethodInfo GetInstanceMethod(Type declaringType, string name, Type[] parameterTypes)
        {
            foreach (MethodInfo method in declaringType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (method.Name == name && IsMatch(method.GetParameters(), parameterTypes)) return method;
            }
            return null;
        }
        internal static MethodInfo GetInstanceMethod(TypeInfo declaringType, string name, Type[] types)
        {
            return GetInstanceMethod(declaringType.AsType(), name, types);
        }
#elif PROFILE259
        internal static MemberInfo GetInstanceMember(TypeInfo declaringType, string name)
        {
            IEnumerable<MemberInfo> members = declaringType.DeclaredMembers;
            IList<MemberInfo> found = new List<MemberInfo>();
            foreach (MemberInfo member in members)
            {
                if (member.Name.Equals(name))
                {
                    found.Add(member);
                }
            }
            switch (found.Count)
            {
                case 0: return null;
                case 1: return found.First();
                default: throw new AmbiguousMatchException(name);
            }
        }
        internal static MethodInfo GetInstanceMethod(Type declaringType, string name)
        {
            var methods = declaringType.GetRuntimeMethods();
            foreach (MethodInfo method in methods)
            {
                if (method.Name == name)
                {
                    return method;
                }
            }
            return null;
        }
        internal static MethodInfo GetInstanceMethod(TypeInfo declaringType, string name)
        {
            return GetInstanceMethod(declaringType.AsType(), name); ;
        }
        internal static MethodInfo GetStaticMethod(Type declaringType, string name)
        {
            var methods = declaringType.GetRuntimeMethods();
            foreach (MethodInfo method in methods)
            {
                if (method.Name == name)
                {
                    return method;
                }
            }
            return null;
        }

        internal static MethodInfo GetStaticMethod(TypeInfo declaringType, string name)
        {
            return GetStaticMethod(declaringType.AsType(), name);
        }
        internal static MethodInfo GetStaticMethod(Type declaringType, string name, Type[] parameterTypes)
        {
            var methods = declaringType.GetRuntimeMethods();
            foreach (MethodInfo method in methods)
            {
                if (method.Name == name &&
                    IsMatch(method.GetParameters(), parameterTypes))
                {
                    return method;
                }
            }
            return null;
        }
        internal static MethodInfo GetInstanceMethod(Type declaringType, string name, Type[] parameterTypes)
        {
            var methods = declaringType.GetRuntimeMethods();
            foreach (MethodInfo method in methods)
            {
                if (method.Name == name &&
                    IsMatch(method.GetParameters(), parameterTypes))
                {
                    return method;
                }
            }
            return null;
        }
        internal static MethodInfo GetInstanceMethod(TypeInfo declaringType, string name, Type[] types)
        {
            return GetInstanceMethod(declaringType.AsType(), name, types);
        }
#else
        internal static MethodInfo GetInstanceMethod(Type declaringType, string name)
        {
            return declaringType.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
        internal static MethodInfo GetStaticMethod(Type declaringType, string name)
        {
            return declaringType.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }
        internal static MethodInfo GetStaticMethod(Type declaringType, string name, Type[] parameterTypes)
        {
#if PORTABLE
            foreach (MethodInfo method in declaringType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (method.Name == name && IsMatch(method.GetParameters(), parameterTypes)) return method;
            }
            return null;
#else
            return declaringType.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, parameterTypes, null);
#endif
        }
        internal static MethodInfo GetInstanceMethod(Type declaringType, string name, Type[] types)
        {
            if (types == null) types = EmptyTypes;
#if PORTABLE || COREFX
            MethodInfo method = declaringType.GetMethod(name, types);
            if (method != null && method.IsStatic) method = null;
            return method;
#else
            return declaringType.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, types, null);
#endif
        }
#endif

        internal static bool IsSubclassOf(Type type, Type baseClass)
        {
#if COREFX || PROFILE259
            return type.GetTypeInfo().IsSubclassOf(baseClass);
#else
            return type.IsSubclassOf(baseClass);
#endif
        }

        public readonly static Type[] EmptyTypes =
#if PORTABLE || CF2 || CF35 || PROFILE259
            new Type[0];
#else
            Type.EmptyTypes;
#endif

#if COREFX || PROFILE259
        private static readonly Type[] knownTypes = new Type[] {
                typeof(bool), typeof(char), typeof(sbyte), typeof(byte),
                typeof(short), typeof(ushort), typeof(int), typeof(uint),
                typeof(long), typeof(ulong), typeof(float), typeof(double),
                typeof(decimal), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid), typeof(Uri),
                typeof(byte[]), typeof(Type)};
        private static readonly ProtoTypeCode[] knownCodes = new ProtoTypeCode[] {
            ProtoTypeCode.Boolean, ProtoTypeCode.Char, ProtoTypeCode.SByte, ProtoTypeCode.Byte,
            ProtoTypeCode.Int16, ProtoTypeCode.UInt16, ProtoTypeCode.Int32, ProtoTypeCode.UInt32,
            ProtoTypeCode.Int64, ProtoTypeCode.UInt64, ProtoTypeCode.Single, ProtoTypeCode.Double,
            ProtoTypeCode.Decimal, ProtoTypeCode.String,
            ProtoTypeCode.DateTime, ProtoTypeCode.TimeSpan, ProtoTypeCode.Guid, ProtoTypeCode.Uri,
            ProtoTypeCode.ByteArray, ProtoTypeCode.Type
        };

#endif

        public static ProtoTypeCode GetTypeCode(Type type)
        {
#if COREFX || PROFILE259
            if (IsEnum(type))
            {
                type = Enum.GetUnderlyingType(type);
            }
            int idx = Array.IndexOf<Type>(knownTypes, type);
            if (idx >= 0) return knownCodes[idx];
            return type == null ? ProtoTypeCode.Empty : ProtoTypeCode.Unknown;
#else
            TypeCode code = Type.GetTypeCode(type);
            switch (code)
            {
                case TypeCode.Empty:
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.DateTime:
                case TypeCode.String:
                    return (ProtoTypeCode)code;
            }
            if (type == typeof(TimeSpan)) return ProtoTypeCode.TimeSpan;
            if (type == typeof(Guid)) return ProtoTypeCode.Guid;
            if (type == typeof(Uri)) return ProtoTypeCode.Uri;
#if PORTABLE
            // In PCLs, the Uri type may not match (WinRT uses Internal/Uri, .Net uses System/Uri), so match on the full name instead
            if (type.FullName == typeof(Uri).FullName) return ProtoTypeCode.Uri;
#endif
            if (type == typeof(byte[])) return ProtoTypeCode.ByteArray;
            if (type == typeof(Type)) return ProtoTypeCode.Type;

            return ProtoTypeCode.Unknown;
#endif
        }

        internal static Type GetUnderlyingType(Type type)
        {
            return Nullable.GetUnderlyingType(type);
        }

        internal static bool IsValueType(Type type)
        {
#if COREFX || PROFILE259
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }
        internal static bool IsSealed(Type type)
        {
#if COREFX || PROFILE259
            return type.GetTypeInfo().IsSealed;
#else
            return type.IsSealed;
#endif
        }
        internal static bool IsClass(Type type)
        {
#if COREFX || PROFILE259
            return type.GetTypeInfo().IsClass;
#else
            return type.IsClass;
#endif
        }

        internal static bool IsEnum(Type type)
        {
#if COREFX || PROFILE259
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        internal static MethodInfo GetGetMethod(PropertyInfo property, bool nonPublic, bool allowInternal)
        {
            if (property == null) return null;
#if COREFX || PROFILE259
            MethodInfo method = property.GetMethod;
            if (!nonPublic && method != null && !method.IsPublic) method = null;
            return method;
#else
            MethodInfo method = property.GetGetMethod(nonPublic);
            if (method == null && !nonPublic && allowInternal)
            { // could be "internal" or "protected internal"; look for a non-public, then back-check
                method = property.GetGetMethod(true);
                if (method == null && !(method.IsAssembly || method.IsFamilyOrAssembly))
                {
                    method = null;
                }
            }
            return method;
#endif
        }
        internal static MethodInfo GetSetMethod(PropertyInfo property, bool nonPublic, bool allowInternal)
        {
            if (property == null) return null;
#if COREFX || PROFILE259
            MethodInfo method = property.SetMethod;
            if (!nonPublic && method != null && !method.IsPublic) method = null;
            return method;
#else
            MethodInfo method = property.GetSetMethod(nonPublic);
            if (method == null && !nonPublic && allowInternal)
            { // could be "internal" or "protected internal"; look for a non-public, then back-check
                method = property.GetGetMethod(true);
                if (method == null && !(method.IsAssembly || method.IsFamilyOrAssembly))
                {
                    method = null;
                }
            }
            return method;
#endif
        }

#if COREFX || PORTABLE || PROFILE259
        private static bool IsMatch(ParameterInfo[] parameters, Type[] parameterTypes)
        {
            if (parameterTypes == null) parameterTypes = EmptyTypes;
            if (parameters.Length != parameterTypes.Length) return false;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType != parameterTypes[i]) return false;
            }
            return true;
        }
#endif
#if COREFX || PROFILE259
        internal static ConstructorInfo GetConstructor(Type type, Type[] parameterTypes, bool nonPublic)
        {
            return GetConstructor(type.GetTypeInfo(), parameterTypes, nonPublic);
        }
        internal static ConstructorInfo GetConstructor(TypeInfo type, Type[] parameterTypes, bool nonPublic)
        {
            return GetConstructors(type, nonPublic).SingleOrDefault(ctor => IsMatch(ctor.GetParameters(), parameterTypes));
        }
        internal static ConstructorInfo[] GetConstructors(TypeInfo typeInfo, bool nonPublic)
        {
            return typeInfo.DeclaredConstructors.Where(c => !c.IsStatic && ((!nonPublic && c.IsPublic) || nonPublic)).ToArray();
        }
        internal static PropertyInfo GetProperty(Type type, string name, bool nonPublic)
        {
            return GetProperty(type.GetTypeInfo(), name, nonPublic);
        }
        internal static PropertyInfo GetProperty(TypeInfo type, string name, bool nonPublic)
        {
            return type.GetDeclaredProperty(name);
        }
#else

        internal static ConstructorInfo GetConstructor(Type type, Type[] parameterTypes, bool nonPublic)
        {
#if PORTABLE || COREFX
            // pretty sure this will only ever return public, but...
            ConstructorInfo ctor = type.GetConstructor(parameterTypes);
            return (ctor != null && (nonPublic || ctor.IsPublic)) ? ctor : null;
#else
            return type.GetConstructor(
                nonPublic ? BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                          : BindingFlags.Instance | BindingFlags.Public,
                    null, parameterTypes, null);
#endif

        }
        internal static ConstructorInfo[] GetConstructors(Type type, bool nonPublic)
        {
            return type.GetConstructors(
                nonPublic ? BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                          : BindingFlags.Instance | BindingFlags.Public);
        }
        internal static PropertyInfo GetProperty(Type type, string name, bool nonPublic)
        {
            return type.GetProperty(name,
                nonPublic ? BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                          : BindingFlags.Instance | BindingFlags.Public);
        }
#endif


        internal static object ParseEnum(Type type, string value)
        {
            return Enum.Parse(type, value, true);
        }


        internal static MemberInfo[] GetInstanceFieldsAndProperties(Type type, bool publicOnly)
        {
#if PROFILE259
            var members = new List<MemberInfo>();
            foreach (FieldInfo field in type.GetRuntimeFields())
            {
                if (field.IsStatic) continue;
                if (field.IsPublic || !publicOnly) members.Add(field);
            }
            foreach (PropertyInfo prop in type.GetRuntimeProperties())
            {
                MethodInfo getter = Helpers.GetGetMethod(prop, true, true);
                if (getter == null || getter.IsStatic) continue;
                if (getter.IsPublic || !publicOnly) members.Add(prop);
            }
            return members.ToArray();
#else
            BindingFlags flags = publicOnly ? BindingFlags.Public | BindingFlags.Instance : BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
            PropertyInfo[] props = type.GetProperties(flags);
            FieldInfo[] fields = type.GetFields(flags);
            MemberInfo[] members = new MemberInfo[fields.Length + props.Length];
            props.CopyTo(members, 0);
            fields.CopyTo(members, props.Length);
            return members;
#endif
        }

        internal static Type GetMemberType(MemberInfo member)
        {
#if PORTABLE || COREFX || PROFILE259
            if (member is PropertyInfo prop) return prop.PropertyType;
            FieldInfo fld = member as FieldInfo;
            return fld?.FieldType;
#else
            switch (member.MemberType)
            {
                case MemberTypes.Field: return ((FieldInfo)member).FieldType;
                case MemberTypes.Property: return ((PropertyInfo)member).PropertyType;
                default: return null;
            }
#endif
        }

        internal static bool IsAssignableFrom(Type target, Type type)
        {
#if PROFILE259
            return target.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
#else
            return target.IsAssignableFrom(type);
#endif
        }
        internal static Assembly GetAssembly(Type type)
        {
#if COREFX || PROFILE259
            return type.GetTypeInfo().Assembly;
#else
            return type.Assembly;
#endif
        }
        internal static byte[] GetBuffer(MemoryStream ms)
        {
#if COREFX
            if(!ms.TryGetBuffer(out var segment))
            {
                throw new InvalidOperationException("Unable to obtain underlying MemoryStream buffer");
            } else if(segment.Offset != 0)
            {
                throw new InvalidOperationException("Underlying MemoryStream buffer was not zero-offset");
            } else
            {
                return segment.Array;
            }
#elif PORTABLE || PROFILE259
            return ms.ToArray();
#else
            return ms.GetBuffer();
#endif
        }
    }
    /// <summary>
    /// Intended to be a direct map to regular TypeCode, but:
    /// - with missing types
    /// - existing on WinRT
    /// </summary>
    internal enum ProtoTypeCode
    {
        Empty = 0,
        Unknown = 1, // maps to TypeCode.Object
        Boolean = 3,
        Char = 4,
        SByte = 5,
        Byte = 6,
        Int16 = 7,
        UInt16 = 8,
        Int32 = 9,
        UInt32 = 10,
        Int64 = 11,
        UInt64 = 12,
        Single = 13,
        Double = 14,
        Decimal = 15,
        DateTime = 16,
        String = 18,

        // additions
        TimeSpan = 100,
        ByteArray = 101,
        Guid = 102,
        Uri = 103,
        Type = 104
    }
}
