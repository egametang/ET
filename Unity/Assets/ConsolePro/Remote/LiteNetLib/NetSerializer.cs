#if DEBUG && !UNITY_WP_8_1 && !UNITY_WSA_8_1
﻿using System;
using System.Reflection;
using System.Collections.Generic;
#if WINRT || NETCORE
using System.Linq;
#endif

namespace FlyingWormConsole3.LiteNetLib.Utils
{
    public interface INetSerializable
    {
        void Serialize(NetDataWriter writer);
        void Desereialize(NetDataReader reader);
    }

    public abstract class NetSerializerHasher
    {
        public abstract ulong GetHash(string type);
        public abstract void WriteHash(ulong hash, NetDataWriter writer);
        public abstract ulong ReadHash(NetDataReader reader);
    }

    public sealed class FNVHasher : NetSerializerHasher
    {
        private readonly Dictionary<string, ulong> _hashCache = new Dictionary<string, ulong>();
        private readonly char[] _hashBuffer = new char[1024];

        public override ulong GetHash(string type)
        {
            ulong hash;
            if (_hashCache.TryGetValue(type, out hash))
            {
                return hash;
            }
            hash = 14695981039346656037UL; //offset
            int len = type.Length;
            type.CopyTo(0, _hashBuffer, 0, len);
            for (var i = 0; i < len; i++)
            {
                hash = hash ^ _hashBuffer[i];
                hash *= 1099511628211UL; //prime
            }
            _hashCache.Add(type, hash);
            return hash;
        }

        public override ulong ReadHash(NetDataReader reader)
        {
            return reader.GetULong();
        }

        public override void WriteHash(ulong hash, NetDataWriter writer)
        {
            writer.Put(hash);
        }
    }

    public sealed class NetSerializer
    {
        private sealed class CustomType
        {
            public readonly CustomTypeWrite WriteDelegate;
            public readonly CustomTypeRead ReadDelegate;

            public CustomType(CustomTypeWrite writeDelegate, CustomTypeRead readDelegate)
            {
                WriteDelegate = writeDelegate;
                ReadDelegate = readDelegate;
            }
        }

        private delegate void CustomTypeWrite(NetDataWriter writer, object customObj);
        private delegate object CustomTypeRead(NetDataReader reader);

        private sealed class StructInfo
        {
            public readonly Action<NetDataWriter>[] WriteDelegate;
            public readonly Action<NetDataReader>[] ReadDelegate;
            public readonly Type[] FieldTypes;
            public object Reference;
            public Func<object> CreatorFunc;
            public Action<object, object> OnReceive;
            public readonly ulong Hash;
            public readonly int MembersCount;

            public StructInfo(ulong hash, int membersCount)
            {
                Hash = hash;
                MembersCount = membersCount;
                WriteDelegate = new Action<NetDataWriter>[membersCount];
                ReadDelegate = new Action<NetDataReader>[membersCount];
                FieldTypes = new Type[membersCount];
            }

            public void Write(NetDataWriter writer, object obj)
            {
                Reference = obj;
                for (int i = 0; i < MembersCount; i++)
                {
                    WriteDelegate[i](writer);
                }
            }

            public void Read(NetDataReader reader)
            {
                for (int i = 0; i < MembersCount; i++)
                {
                    ReadDelegate[i](reader);
                }
            }
        }

        private readonly Dictionary<ulong, StructInfo> _cache;
        private readonly Dictionary<Type, CustomType> _registeredCustomTypes;

        private static readonly HashSet<Type> BasicTypes = new HashSet<Type>
        {
            typeof(int),
            typeof(uint),
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(long),
            typeof(ulong),
            typeof(string),
            typeof(float),
            typeof(double),
            typeof(bool)
        };

        private readonly NetDataWriter _writer;
        private readonly NetSerializerHasher _hasher;
        private const int MaxStringLenght = 1024;

        public NetSerializer() : this(new FNVHasher())
        {
        }

        public NetSerializer(NetSerializerHasher hasher)
        {
            _hasher = hasher;
            _cache = new Dictionary<ulong, StructInfo>();
            _registeredCustomTypes = new Dictionary<Type, CustomType>();
            _writer = new NetDataWriter();
        }

        private bool RegisterCustomTypeInternal<T>(Func<T> constructor) where T : INetSerializable
        {
            var t = typeof(T);
            if (_registeredCustomTypes.ContainsKey(t))
            {
                return false;
            }

            var rwDelegates = new CustomType(
                (writer, obj) =>
                {
                    ((T)obj).Serialize(writer);
                },
                reader =>
                {
                    var instance = constructor();
                    instance.Desereialize(reader);
                    return instance;
                });
            _registeredCustomTypes.Add(t, rwDelegates);
            return true;
        }

        /// <summary>
        /// Register custom property type
        /// </summary>
        /// <typeparam name="T">INetSerializable structure</typeparam>
        /// <returns>True - if register successful, false - if type already registered</returns>
        public bool RegisterCustomType<T>() where T : struct, INetSerializable
        {
            return RegisterCustomTypeInternal(() => new T());
        }

        /// <summary>
        /// Register custom property type
        /// </summary>
        /// <typeparam name="T">INetSerializable class</typeparam>
        /// <returns>True - if register successful, false - if type already registered</returns>
        public bool RegisterCustomType<T>(Func<T> constructor) where T : class, INetSerializable
        {
            return RegisterCustomTypeInternal(constructor);
        }

        /// <summary>
        /// Register custom property type
        /// </summary>
        /// <param name="writeDelegate"></param>
        /// <param name="readDelegate"></param>
        /// <returns>True - if register successful, false - if type already registered</returns>
        public bool RegisterCustomType<T>(Action<NetDataWriter, T> writeDelegate, Func<NetDataReader, T> readDelegate)
        {
            var t = typeof(T);
            if (BasicTypes.Contains(t) || _registeredCustomTypes.ContainsKey(t))
            {
                return false;
            }

            var rwDelegates = new CustomType(
                (writer, obj) => writeDelegate(writer, (T)obj),
                reader => readDelegate(reader));

            _registeredCustomTypes.Add(t, rwDelegates);
            return true;
        }

        private static Delegate CreateDelegate(Type type, MethodInfo info)
        {
#if WINRT || NETCORE
            return info.CreateDelegate(type);
#else
            return Delegate.CreateDelegate(type, info);
#endif
        }

        private static Func<TClass, TProperty> ExtractGetDelegate<TClass, TProperty>(MethodInfo info)
        {
            return (Func<TClass, TProperty>)CreateDelegate(typeof(Func<TClass, TProperty>), info);
        }

        private static Action<TClass, TProperty> ExtractSetDelegate<TClass, TProperty>(MethodInfo info)
        {
            return (Action<TClass, TProperty>)CreateDelegate(typeof(Action<TClass, TProperty>), info);
        }

        private StructInfo RegisterInternal<T>() where T : class
        {
            Type t = typeof(T);
            ulong nameHash = _hasher.GetHash(t.Name);

            StructInfo info;
            if (_cache.TryGetValue(nameHash, out info))
            {
                return info;
            }

#if WINRT || NETCORE
            var props = t.GetRuntimeProperties().ToArray();
            int propsCount = props.Count();
#else
            var props = t.GetProperties(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.GetProperty |
                BindingFlags.SetProperty);
            int propsCount = props.Length;
#endif
            if (props == null || propsCount < 0)
            {
                throw new ArgumentException("Type does not contain acceptable fields");
            }

            info = new StructInfo(nameHash, propsCount);
            for (int i = 0; i < props.Length; i++)
            {
                var property = props[i];
                var propertyType = property.PropertyType;

                //Set field type
                info.FieldTypes[i] = propertyType.IsArray ? propertyType.GetElementType() : propertyType;
#if WINRT || NETCORE
                bool isEnum = propertyType.GetTypeInfo().IsEnum;
                var getMethod = property.GetMethod;
                var setMethod = property.SetMethod;
#else
                bool isEnum = propertyType.IsEnum;
                var getMethod = property.GetGetMethod();
                var setMethod = property.GetSetMethod();
#endif
                if (isEnum)
                {
                    var underlyingType = Enum.GetUnderlyingType(propertyType);
                    if (underlyingType == typeof(byte))
                    {
                        info.ReadDelegate[i] = reader =>
                        {
                            property.SetValue(info.Reference, Enum.ToObject(propertyType, reader.GetByte()), null);
                        };
                        info.WriteDelegate[i] = writer =>
                        {
                            writer.Put((byte)property.GetValue(info.Reference, null));
                        };
                    }
                    else if (underlyingType == typeof(int))
                    {
                        info.ReadDelegate[i] = reader =>
                        {
                            property.SetValue(info.Reference, Enum.ToObject(propertyType, reader.GetInt()), null);
                        };
                        info.WriteDelegate[i] = writer =>
                        {
                            writer.Put((int)property.GetValue(info.Reference, null));
                        };
                    }
                    else
                    {
                        throw new Exception("Not supported enum underlying type: " + underlyingType.Name);
                    }
                }
                else if (propertyType == typeof(string))
                {
                    var setDelegate = ExtractSetDelegate<T, string>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, string>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetString(MaxStringLenght));
                    info.WriteDelegate[i] = writer => writer.Put(getDelegate((T)info.Reference), MaxStringLenght);
                }
                else if (propertyType == typeof(bool))
                {
                    var setDelegate = ExtractSetDelegate<T, bool>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, bool>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetBool());
                    info.WriteDelegate[i] = writer => writer.Put(getDelegate((T)info.Reference));
                }
                else if (propertyType == typeof(byte))
                {
                    var setDelegate = ExtractSetDelegate<T, byte>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, byte>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetByte());
                    info.WriteDelegate[i] = writer => writer.Put(getDelegate((T)info.Reference));
                }
                else if (propertyType == typeof(sbyte))
                {
                    var setDelegate = ExtractSetDelegate<T, sbyte>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, sbyte>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetSByte());
                    info.WriteDelegate[i] = writer => writer.Put(getDelegate((T)info.Reference));
                }
                else if (propertyType == typeof(short))
                {
                    var setDelegate = ExtractSetDelegate<T, short>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, short>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetShort());
                    info.WriteDelegate[i] = writer => writer.Put(getDelegate((T)info.Reference));
                }
                else if (propertyType == typeof(ushort))
                {
                    var setDelegate = ExtractSetDelegate<T, ushort>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, ushort>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetUShort());
                    info.WriteDelegate[i] = writer => writer.Put(getDelegate((T)info.Reference));
                }
                else if (propertyType == typeof(int))
                {
                    var setDelegate = ExtractSetDelegate<T, int>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, int>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetInt());
                    info.WriteDelegate[i] = writer => writer.Put(getDelegate((T)info.Reference));
                }
                else if (propertyType == typeof(uint))
                {
                    var setDelegate = ExtractSetDelegate<T, uint>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, uint>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetUInt());
                    info.WriteDelegate[i] = writer => writer.Put(getDelegate((T)info.Reference));
                }
                else if (propertyType == typeof(long))
                {
                    var setDelegate = ExtractSetDelegate<T, long>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, long>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetLong());
                    info.WriteDelegate[i] = writer => writer.Put(getDelegate((T)info.Reference));
                }
                else if (propertyType == typeof(ulong))
                {
                    var setDelegate = ExtractSetDelegate<T, ulong>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, ulong>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetULong());
                    info.WriteDelegate[i] = writer => writer.Put(getDelegate((T)info.Reference));
                }
                else if (propertyType == typeof(float))
                {
                    var setDelegate = ExtractSetDelegate<T, float>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, float>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetFloat());
                    info.WriteDelegate[i] = writer => writer.Put(getDelegate((T)info.Reference));
                }
                else if (propertyType == typeof(double))
                {
                    var setDelegate = ExtractSetDelegate<T, double>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, double>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetDouble());
                    info.WriteDelegate[i] = writer => writer.Put(getDelegate((T)info.Reference));
                }
                // Array types
                else if (propertyType == typeof(string[]))
                {
                    var setDelegate = ExtractSetDelegate<T, string[]>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, string[]>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetStringArray(MaxStringLenght));
                    info.WriteDelegate[i] = writer => writer.PutArray(getDelegate((T)info.Reference), MaxStringLenght);
                }
                else if (propertyType == typeof(byte[]))
                {
                    var setDelegate = ExtractSetDelegate<T, byte[]>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, byte[]>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetBytesWithLength());
                    info.WriteDelegate[i] = writer => writer.PutBytesWithLength(getDelegate((T)info.Reference));
                }
                else if (propertyType == typeof(short[]))
                {
                    var setDelegate = ExtractSetDelegate<T, short[]>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, short[]>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetShortArray());
                    info.WriteDelegate[i] = writer => writer.PutArray(getDelegate((T)info.Reference));
                }
                else if (propertyType == typeof(ushort[]))
                {
                    var setDelegate = ExtractSetDelegate<T, ushort[]>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, ushort[]>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetUShortArray());
                    info.WriteDelegate[i] = writer => writer.PutArray(getDelegate((T)info.Reference));
                }
                else if (propertyType == typeof(int[]))
                {
                    var setDelegate = ExtractSetDelegate<T, int[]>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, int[]>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetIntArray());
                    info.WriteDelegate[i] = writer => writer.PutArray(getDelegate((T)info.Reference));
                }
                else if (propertyType == typeof(uint[]))
                {
                    var setDelegate = ExtractSetDelegate<T, uint[]>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, uint[]>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetUIntArray());
                    info.WriteDelegate[i] = writer => writer.PutArray(getDelegate((T)info.Reference));
                }
                else if (propertyType == typeof(long[]))
                {
                    var setDelegate = ExtractSetDelegate<T, long[]>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, long[]>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetLongArray());
                    info.WriteDelegate[i] = writer => writer.PutArray(getDelegate((T)info.Reference));
                }
                else if (propertyType == typeof(ulong[]))
                {
                    var setDelegate = ExtractSetDelegate<T, ulong[]>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, ulong[]>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetULongArray());
                    info.WriteDelegate[i] = writer => writer.PutArray(getDelegate((T)info.Reference));
                }
                else if (propertyType == typeof(float[]))
                {
                    var setDelegate = ExtractSetDelegate<T, float[]>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, float[]>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetFloatArray());
                    info.WriteDelegate[i] = writer => writer.PutArray(getDelegate((T)info.Reference));
                }
                else if (propertyType == typeof(double[]))
                {
                    var setDelegate = ExtractSetDelegate<T, double[]>(setMethod);
                    var getDelegate = ExtractGetDelegate<T, double[]>(getMethod);
                    info.ReadDelegate[i] = reader => setDelegate((T)info.Reference, reader.GetDoubleArray());
                    info.WriteDelegate[i] = writer => writer.PutArray(getDelegate((T)info.Reference));
                }
                else
                {
                    CustomType registeredCustomType;
                    bool array = false;

                    if (propertyType.IsArray)
                    {
                        array = true;
                        propertyType = propertyType.GetElementType();
                    }

                    if (_registeredCustomTypes.TryGetValue(propertyType, out registeredCustomType))
                    {
                        if (array) //Array type serialize/deserialize
                        {
                            info.ReadDelegate[i] = reader =>
                            {
                                ushort arrLength = reader.GetUShort();
                                Array arr = Array.CreateInstance(propertyType, arrLength);
                                for (int k = 0; k < arrLength; k++)
                                {
                                    arr.SetValue(registeredCustomType.ReadDelegate(reader), k);
                                }

                                property.SetValue(info.Reference, arr, null);
                            };

                            info.WriteDelegate[i] = writer =>
                            {
                                Array arr = (Array)property.GetValue(info.Reference, null);
                                writer.Put((ushort)arr.Length);
                                for (int k = 0; k < arr.Length; k++)
                                {
                                    registeredCustomType.WriteDelegate(writer, arr.GetValue(k));
                                }
                            };
                        }
                        else //Simple
                        {
                            info.ReadDelegate[i] = reader =>
                            {
                                property.SetValue(info.Reference, registeredCustomType.ReadDelegate(reader), null);
                            };

                            info.WriteDelegate[i] = writer =>
                            {
                                registeredCustomType.WriteDelegate(writer, property.GetValue(info.Reference, null));
                            };
                        }
                    }
                    else
                    {
                        throw new Exception("Unknown property type: " + propertyType.Name);
                    }
                }
            }
            _cache.Add(nameHash, info);

            return info;
        }

        /// <summary>
        /// Reads all available data from NetDataReader and calls OnReceive delegates
        /// </summary>
        /// <param name="reader">NetDataReader with packets data</param>
        public void ReadAllPackets(NetDataReader reader)
        {
            while (reader.AvailableBytes > 0)
            {
                ReadPacket(reader);
            }
        }

        /// <summary>
        /// Reads all available data from NetDataReader and calls OnReceive delegates
        /// </summary>
        /// <param name="reader">NetDataReader with packets data</param>
        /// <param name="userData">Argument that passed to OnReceivedEvent</param>
        public void ReadAllPackets<T>(NetDataReader reader, T userData)
        {
            while (reader.AvailableBytes > 0)
            {
                ReadPacket(reader, userData);
            }
        }

        /// <summary>
        /// Reads one packet from NetDataReader and calls OnReceive delegate
        /// </summary>
        /// <param name="reader">NetDataReader with packet</param>
        public void ReadPacket(NetDataReader reader)
        {
            ReadPacket(reader, null);
        }

        private StructInfo ReadInfo(NetDataReader reader)
        {
            ulong hash = _hasher.ReadHash(reader);
            StructInfo info;
            if (!_cache.TryGetValue(hash, out info))
            {
                throw new Exception("Undefined packet received");
            }
            return info;
        }

        /// <summary>
        /// Reads packet with known type
        /// </summary>
        /// <param name="reader">NetDataReader with packet</param>
        /// <returns>Returns packet if packet in reader is matched type</returns>
        public T ReadKnownPacket<T>(NetDataReader reader) where T : class, new()
        {
            var info = ReadInfo(reader);
            ulong typeHash = _hasher.GetHash(typeof(T).Name);
            if (typeHash != info.Hash)
            {
                return null;
            }
            info.Reference = info.CreatorFunc != null ? info.CreatorFunc() : Activator.CreateInstance<T>();
            info.Read(reader);
            return (T)info.Reference;
        }

        /// <summary>
        /// Reads packet with known type (non alloc variant)
        /// </summary>
        /// <param name="reader">NetDataReader with packet</param>
        /// <param name="target">Deserialization target</param>
        /// <returns>Returns true if packet in reader is matched type</returns>
        public bool ReadKnownPacket<T>(NetDataReader reader, T target) where T : class, new()
        {
            var info = ReadInfo(reader);
            ulong typeHash = _hasher.GetHash(typeof(T).Name);
            if (typeHash != info.Hash)
            {
                return false;
            }

            info.Reference = target;
            info.Read(reader);
            return true;
        }

        /// <summary>
        /// Reads one packet from NetDataReader and calls OnReceive delegate
        /// </summary>
        /// <param name="reader">NetDataReader with packet</param>
        /// <param name="userData">Argument that passed to OnReceivedEvent</param>
        public void ReadPacket(NetDataReader reader, object userData)
        {
            var info = ReadInfo(reader);
            if (info.CreatorFunc != null)
            {
                info.Reference = info.CreatorFunc();
            }
            info.Read(reader);
            info.OnReceive(info.Reference, userData);
        }

        /// <summary>
        /// Register and subscribe to packet receive event
        /// </summary>
        /// <param name="onReceive">event that will be called when packet deserialized with ReadPacket method</param>
        /// <param name="packetConstructor">Method that constructs packet intead of slow Activator.CreateInstance</param>
        public void Subscribe<T>(Action<T> onReceive, Func<T> packetConstructor) where T : class, new()
        {
            var info = RegisterInternal<T>();
            info.CreatorFunc = () => packetConstructor();
            info.OnReceive = (o, userData) => { onReceive((T)o); };
        }

        /// <summary>
        /// Register packet type for direct reading (ReadKnownPacket)
        /// </summary>
        /// <param name="packetConstructor">Method that constructs packet intead of slow Activator.CreateInstance</param>
        public void Register<T>(Func<T> packetConstructor = null) where T : class, new()
        {
            var info = RegisterInternal<T>();
            if (packetConstructor != null)
            {
                info.CreatorFunc = () => packetConstructor();
            }
            info.OnReceive = (o, userData) => { };
        }

        /// <summary>
        /// Register and subscribe to packet receive event (with userData)
        /// </summary>
        /// <param name="onReceive">event that will be called when packet deserialized with ReadPacket method</param>
        /// <param name="packetConstructor">Method that constructs packet intead of slow Activator.CreateInstance</param>
        public void Subscribe<T, TUserData>(Action<T, TUserData> onReceive, Func<T> packetConstructor) where T : class, new()
        {
            var info = RegisterInternal<T>();
            info.CreatorFunc = () => packetConstructor();
            info.OnReceive = (o, userData) => { onReceive((T)o, (TUserData)userData); };
        }

        /// <summary>
        /// Register and subscribe to packet receive event
        /// This metod will overwrite last received packet class on receive (less garbage)
        /// </summary>
        /// <param name="onReceive">event that will be called when packet deserialized with ReadPacket method</param>
        public void SubscribeReusable<T>(Action<T> onReceive) where T : class, new()
        {
            var info = RegisterInternal<T>();
            info.Reference = new T();
            info.OnReceive = (o, userData) => { onReceive((T)o); };
        }

        /// <summary>
        /// Register and subscribe to packet receive event
        /// This metod will overwrite last received packet class on receive (less garbage)
        /// </summary>
        /// <param name="onReceive">event that will be called when packet deserialized with ReadPacket method</param>
        public void SubscribeReusable<T, TUserData>(Action<T, TUserData> onReceive) where T : class, new()
        {
            var info = RegisterInternal<T>();
            info.Reference = new T();
            info.OnReceive = (o, userData) => { onReceive((T)o, (TUserData)userData); };
        }

        /// <summary>
        /// Serialize struct to NetDataWriter (fast)
        /// </summary>
        /// <param name="writer">Serialization target NetDataWriter</param>
        /// <param name="obj">Struct to serialize</param>
        public void Serialize<T>(NetDataWriter writer, T obj) where T : class, new()
        {
            var info = RegisterInternal<T>();
            _hasher.WriteHash(info.Hash, writer);
            info.Write(writer, obj);
        }

        /// <summary>
        /// Serialize struct to byte array
        /// </summary>
        /// <param name="obj">Struct to serialize</param>
        /// <returns>byte array with serialized data</returns>
        public byte[] Serialize<T>(T obj) where T : class, new()
        {
            _writer.Reset();
            Serialize(_writer, obj);
            return _writer.CopyData();
        }
    }
}
#endif
