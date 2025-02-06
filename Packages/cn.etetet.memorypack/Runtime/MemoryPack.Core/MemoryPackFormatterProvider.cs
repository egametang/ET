using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using MemoryPack.Formatters;
using MemoryPack.Internal;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace MemoryPack {

// This service provider is not extension point, for wellknown types
// and fallback if can't resolve in compile time(like generics).
// Therefore, unlike MessagePack for C#, it is static and not extensible.

public static partial class MemoryPackFormatterProvider
{
    // for nongenerics methods
    static readonly ConcurrentDictionary<Type, IMemoryPackFormatter> formatters = new ConcurrentDictionary<Type, IMemoryPackFormatter>(Environment.ProcessorCount, 150);

    // custom generic formatters
    static readonly ConcurrentDictionary<Type, Type> genericFormatterFactory = new ConcurrentDictionary<Type, Type>();

    // custom generic collection formatters
    static readonly ConcurrentDictionary<Type, Type> genericCollectionFormatterFactory = new ConcurrentDictionary<Type, Type>();

    // generics known types
    static readonly Dictionary<Type, Type> KnownGenericTypeFormatters = new Dictionary<Type, Type>(3)
    {
        { typeof(KeyValuePair<,>), typeof(KeyValuePairFormatter<,>) },
        { typeof(Lazy<>), typeof(LazyFormatter<>) },
        { typeof(Nullable<>), typeof(NullableFormatter<>) },
    };

    static partial void RegisterInitialFormatters();

    static MemoryPackFormatterProvider()
    {
        // Initialize on startup
        RegisterWellKnownTypesFormatters();
        // Extension for Unity or others
        RegisterInitialFormatters();
    }

    public static bool IsRegistered<T>() => Check<T>.registered;

    public static void Register<T>(MemoryPackFormatter<T> formatter)
    {
        Check<T>.registered = true; // avoid to call Cache() constructor called.
        formatters[typeof(T)] = formatter;
        Cache<T>.formatter = formatter;
    }

#if NET7_0_OR_GREATER

    public static void Register<T>()
        where T : IMemoryPackFormatterRegister
    {
        T.RegisterFormatter();
    }

#endif

    public static void RegisterGenericType(Type genericType, Type genericFormatterType)
    {
        if (genericType.IsGenericType && genericFormatterType.IsGenericType)
        {
            genericFormatterFactory[genericType] = genericFormatterType;
        }
        else
        {
            MemoryPackSerializationException.ThrowMessage($"Registered type is not generic type. genericType:{genericType.FullName}, formatterType:{genericFormatterType.FullName}");
        }
    }

    public static void RegisterCollection<TCollection, TElement>()
        where TCollection : ICollection<TElement?>, new()
    {
        Register(new GenericCollectionFormatter<TCollection, TElement>());
    }

    public static void RegisterCollection(Type genericCollectionType)
    {
        if (genericCollectionType.IsGenericType && genericCollectionType.GetGenericArguments().Length == 1)
        {
            genericCollectionFormatterFactory[genericCollectionType] = typeof(GenericCollectionFormatter<,>);
        }
        else
        {
            MemoryPackSerializationException.ThrowMessage($"Registered generic collection is not filled generic formatter constraint. type: {genericCollectionType.FullName}");
        }
    }

    public static void RegisterSet<TSet, TElement>()
        where TSet : ISet<TElement?>, new()
    {
        Register(new GenericSetFormatter<TSet, TElement>());
    }

    public static void RegisterSet(Type genericSetType)
    {
        if (genericSetType.IsGenericType && genericSetType.GetGenericArguments().Length == 1)
        {
            genericCollectionFormatterFactory[genericSetType] = typeof(GenericSetFormatter<,>);
        }
        else
        {
            MemoryPackSerializationException.ThrowMessage($"Registered generic set is not filled generic formatter constraint. type: {genericSetType.FullName}");
        }
    }

    public static void RegisterDictionary<TDictionary, TKey, TValue>()
            where TKey : notnull
            where TDictionary : IDictionary<TKey, TValue?>, new()
    {
        Register(new GenericDictionaryFormatter<TDictionary, TKey, TValue>());
    }

    public static void RegisterDictionary(Type genericDictionaryType)
    {
        if (genericDictionaryType.IsGenericType && genericDictionaryType.GetGenericArguments().Length == 2)
        {
            genericCollectionFormatterFactory[genericDictionaryType] = typeof(GenericDictionaryFormatter<,,>);
        }
        else
        {
            MemoryPackSerializationException.ThrowMessage($"Registered generic collection is not filled generic formatter constraint. type: {genericDictionaryType.FullName}");
        }
    }

    // almostly get from Writer/Reader
    // in future, will change static provider to instance provider.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static MemoryPackFormatter<T> GetFormatter<T>()
    {
        return Cache<T>.formatter;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IMemoryPackFormatter GetFormatter(Type type)
    {
        if (formatters.TryGetValue(type, out var formatter))
        {
            return formatter;
        }

        if (TryInvokeRegisterFormatter(type))
        {
            // try again
            if (formatters.TryGetValue(type, out formatter))
            {
                return formatter;
            }
        }

        if (TypeHelpers.IsAnonymous(type))
        {
            formatter = new ErrorMemoryPackFormatter(type, "Serialize anonymous type is not supported, use record or tuple instead.");
            goto END;
        }

        // non registered, try to create generic formatter
        // can not detect IsReferenceOrContainsReference but it only uses array type select so safe).
        var formatter2 = CreateGenericFormatter(type, typeIsReferenceOrContainsReferences: true) as IMemoryPackFormatter;
        if (formatter2 == null)
        {
            formatter2 = new ErrorMemoryPackFormatter(type);
        }
        formatter = formatter2;

    END:
        formatters[type] = formatter;
        return formatter;
    }

    static bool TryInvokeRegisterFormatter(Type type)
    {
        if (typeof(IMemoryPackFormatterRegister).IsAssignableFrom(type))
        {
            // currently C# can not call like `if (T is IMemoryPackFormatterRegister) T.RegisterFormatter()`, so use reflection instead.
            var m = type.GetMethod("MemoryPack.IMemoryPackFormatterRegister.RegisterFormatter", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (m == null)
            {
                // Roslyn3.11 generator generate public static method
                m = type.GetMethod("RegisterFormatter", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            }
            if (m == null)
            {
                throw new InvalidOperationException("Type implements IMemoryPackFormatterRegister but can not found RegisterFormatter. Type: " + type.FullName);
            }
            m!.Invoke(null, null); // Cache<T>.formatter will set from method
            return true;
        }

        return false;
    }

    static class Check<T>
    {
        public static bool registered;
    }

    static class Cache<T>
    {
        public static MemoryPackFormatter<T> formatter = default!;

        static Cache()
        {
            if (Check<T>.registered) return;

            try
            {
                var type = typeof(T);
                if (TryInvokeRegisterFormatter(type))
                {
                    return;
                }

                if (TypeHelpers.IsAnonymous(type))
                {
                    formatter = new ErrorMemoryPackFormatter<T>("Serialize anonymous type is not supported, use record or tuple instead.");
                    goto END;
                }

                var typeIsReferenceOrContainsReferences = RuntimeHelpers.IsReferenceOrContainsReferences<T>();
                var f = CreateGenericFormatter(type, typeIsReferenceOrContainsReferences) as MemoryPackFormatter<T>;

                formatter = f ?? new ErrorMemoryPackFormatter<T>();
            }
            catch (Exception ex)
            {
                formatter = new ErrorMemoryPackFormatter<T>(ex);
            }

        END:
            formatters[typeof(T)] = formatter;
            Check<T>.registered = true;
        }
    }

    internal static object? CreateGenericFormatter(Type type, bool typeIsReferenceOrContainsReferences)
    {
        Type? formatterType = null;

        if (type.IsArray)
        {
            if (type.IsSZArray)
            {
                if (!typeIsReferenceOrContainsReferences)
                {
                    formatterType = typeof(DangerousUnmanagedArrayFormatter<>).MakeGenericType(type.GetElementType()!);
                    goto CREATE;
                }
                else
                {
                    formatterType = typeof(ArrayFormatter<>).MakeGenericType(type.GetElementType()!);
                    goto CREATE;
                }
            }
            else
            {
                var rank = type.GetArrayRank();
                switch (rank)
                {
                    case 2:
                        formatterType = typeof(TwoDimensionalArrayFormatter<>).MakeGenericType(type.GetElementType()!);
                        goto CREATE;
                    case 3:
                        formatterType = typeof(ThreeDimensionalArrayFormatter<>).MakeGenericType(type.GetElementType()!);
                        goto CREATE;
                    case 4:
                        formatterType = typeof(FourDimensionalArrayFormatter<>).MakeGenericType(type.GetElementType()!);
                        goto CREATE;
                    default:
                        return null; // not supported
                }
            }
        }

        if (type.IsEnum || !typeIsReferenceOrContainsReferences)
        {
            formatterType = typeof(DangerousUnmanagedFormatter<>).MakeGenericType(type);
            goto CREATE;
        }

        formatterType = TryCreateGenericFormatterType(type, TupleFormatterTypes.TupleFormatters);
        if (formatterType != null) goto CREATE;

        formatterType = TryCreateGenericFormatterType(type, KnownGenericTypeFormatters);
        if (formatterType != null) goto CREATE;

        formatterType = TryCreateGenericFormatterType(type, ArrayLikeFormatters);
        if (formatterType != null) goto CREATE;

        formatterType = TryCreateGenericFormatterType(type, CollectionFormatters);
        if (formatterType != null) goto CREATE;

#if !UNITY_2021_2_OR_NEWER
        formatterType = TryCreateGenericFormatterType(type, ImmutableCollectionFormatters);
        if (formatterType != null) goto CREATE;
#endif

        formatterType = TryCreateGenericFormatterType(type, InterfaceCollectionFormatters);
        if (formatterType != null) goto CREATE;

        // finally custom generated
        formatterType = TryCreateGenericFormatterType(type, genericFormatterFactory);
        if (formatterType != null) goto CREATE;

        // genericCollectionFormatterFactory
        formatterType = TryCreateGenericCollectionFormatterType(type);
        if (formatterType != null) goto CREATE;

        // Can't resolve formatter, return null(will create ErrorMemoryPackFormatter<T>).
        return null;

    CREATE:
        return Activator.CreateInstance(formatterType);
    }

    static Type? TryCreateGenericFormatterType(Type type, IDictionary<Type, Type> knownTypes)
    {
        if (type.IsGenericType)
        {
            var genericDefinition = type.GetGenericTypeDefinition();

            if (knownTypes.TryGetValue(genericDefinition, out var formatterType))
            {
                return formatterType.MakeGenericType(type.GetGenericArguments());
            }
        }

        return null;
    }

    static Type? TryCreateGenericCollectionFormatterType(Type type)
    {
        if (type.IsGenericType && genericCollectionFormatterFactory.TryGetValue(type, out var formatterType))
        {
            var genericDefinition = type.GetGenericTypeDefinition();
            var elementTypes = genericDefinition.GetGenericArguments();

            // formatterType is <TCollection, TArgs> so concat type at first
            return formatterType.MakeGenericType(elementTypes.Prepend(type).ToArray());
        }

        return null;
    }
}

internal sealed class ErrorMemoryPackFormatter : IMemoryPackFormatter
{
    readonly Type type;
    readonly string? message;

    public ErrorMemoryPackFormatter(Type type)
    {
        this.type = type;
        this.message = null;
    }

    public ErrorMemoryPackFormatter(Type type, string message)
    {
        this.type = type;
        this.message = message;
    }

    public void Serialize(ref MemoryPackWriter writer, ref object? value)
#if NET7_0_OR_GREATER
        
#else
        
#endif        
    {
        Throw();
    }

    public void Deserialize(ref MemoryPackReader reader, ref object? value)
    {
        Throw();
    }

    [DoesNotReturn]
    void Throw()
    {
        if (message != null)
        {
            MemoryPackSerializationException.ThrowMessage(message);
        }
        else
        {
            MemoryPackSerializationException.ThrowNotRegisteredInProvider(type);
        }
    }
}

internal sealed class ErrorMemoryPackFormatter<T> : MemoryPackFormatter<T>
{
    readonly Exception? exception;
    readonly string? message;

    public ErrorMemoryPackFormatter()
    {
        this.exception = null;
        this.message = null;
    }

    public ErrorMemoryPackFormatter(Exception exception)
    {
        this.exception = exception;
        this.message = null;
    }

    public ErrorMemoryPackFormatter(string message)
    {
        this.exception = null;
        this.message = message;
    }

    public override void Serialize(ref MemoryPackWriter writer, ref T? value)
    {
        Throw();
    }

    public override void Deserialize(ref MemoryPackReader reader, ref T? value)
    {
        Throw();
    }

    [DoesNotReturn]
    void Throw()
    {
        if (exception != null)
        {
            MemoryPackSerializationException.ThrowRegisterInProviderFailed(typeof(T), exception);
        }
        else if (message != null)
        {
            MemoryPackSerializationException.ThrowMessage(message);
        }
        else
        {
            MemoryPackSerializationException.ThrowNotRegisteredInProvider(typeof(T));
        }
    }
}

}