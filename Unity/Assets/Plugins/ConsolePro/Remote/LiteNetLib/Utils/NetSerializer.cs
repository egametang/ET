using System;
using System.Reflection;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;

namespace FlyingWormConsole3.LiteNetLib.Utils
{
    public class InvalidTypeException : ArgumentException
    {
        public InvalidTypeException(string message) : base(message) { }
    }

    public class ParseException : Exception
    {
        public ParseException(string message) : base(message) { }
    }

    public class NetSerializer
    {
        private enum CallType
        {
            Basic,
            Array,
            List
        }

        private abstract class FastCall<T>
        {
            public CallType Type;
            public virtual void Init(MethodInfo getMethod, MethodInfo setMethod, CallType type) { Type = type; }
            public abstract void Read(T inf, NetDataReader r);
            public abstract void Write(T inf, NetDataWriter w);
            public abstract void ReadArray(T inf, NetDataReader r);
            public abstract void WriteArray(T inf, NetDataWriter w);
            public abstract void ReadList(T inf, NetDataReader r);
            public abstract void WriteList(T inf, NetDataWriter w);
        }

        private abstract class FastCallSpecific<TClass, TProperty> : FastCall<TClass>
        {
            protected Func<TClass, TProperty> Getter;
            protected Action<TClass, TProperty> Setter;
            protected Func<TClass, TProperty[]> GetterArr;
            protected Action<TClass, TProperty[]> SetterArr;
            protected Func<TClass, List<TProperty>> GetterList;
            protected Action<TClass, List<TProperty>> SetterList;

            public override void ReadArray(TClass inf, NetDataReader r) { throw new InvalidTypeException("Unsupported type: " + typeof(TProperty) + "[]"); }
            public override void WriteArray(TClass inf, NetDataWriter w) { throw new InvalidTypeException("Unsupported type: " + typeof(TProperty) + "[]"); }
            public override void ReadList(TClass inf, NetDataReader r) { throw new InvalidTypeException("Unsupported type: List<" + typeof(TProperty) + ">"); }
            public override void WriteList(TClass inf, NetDataWriter w) { throw new InvalidTypeException("Unsupported type: List<" + typeof(TProperty) + ">"); }

            protected TProperty[] ReadArrayHelper(TClass inf, NetDataReader r)
            {
                ushort count = r.GetUShort();
                var arr = GetterArr(inf);
                arr = arr == null || arr.Length != count ? new TProperty[count] : arr;
                SetterArr(inf, arr);
                return arr;
            }

            protected TProperty[] WriteArrayHelper(TClass inf, NetDataWriter w)
            {
                var arr = GetterArr(inf);
                w.Put((ushort)arr.Length);
                return arr;
            }

            protected List<TProperty> ReadListHelper(TClass inf, NetDataReader r, out int len)
            {
                len = r.GetUShort();
                var list = GetterList(inf);
                if (list == null)
                {
                    list = new List<TProperty>(len);
                    SetterList(inf, list);
                }
                return list;
            }

            protected List<TProperty> WriteListHelper(TClass inf, NetDataWriter w, out int len)
            {
                var list = GetterList(inf);
                if (list == null)
                {
                    len = 0;
                    w.Put(0);
                    return null;
                }
                len = list.Count;
                w.Put((ushort)len);
                return list;
            }

            public override void Init(MethodInfo getMethod, MethodInfo setMethod, CallType type)
            {
                base.Init(getMethod, setMethod, type);
                switch (type)
                {
                    case CallType.Array:
                        GetterArr = (Func<TClass, TProperty[]>)Delegate.CreateDelegate(typeof(Func<TClass, TProperty[]>), getMethod);
                        SetterArr = (Action<TClass, TProperty[]>)Delegate.CreateDelegate(typeof(Action<TClass, TProperty[]>), setMethod);
                        break;
                    case CallType.List:
                        GetterList = (Func<TClass, List<TProperty>>)Delegate.CreateDelegate(typeof(Func<TClass, List<TProperty>>), getMethod);
                        SetterList = (Action<TClass, List<TProperty>>)Delegate.CreateDelegate(typeof(Action<TClass, List<TProperty>>), setMethod);
                        break;
                    default:
                        Getter = (Func<TClass, TProperty>)Delegate.CreateDelegate(typeof(Func<TClass, TProperty>), getMethod);
                        Setter = (Action<TClass, TProperty>)Delegate.CreateDelegate(typeof(Action<TClass, TProperty>), setMethod);
                        break;
                }
            }
        }

        private abstract class FastCallSpecificAuto<TClass, TProperty> : FastCallSpecific<TClass, TProperty>
        {
            protected abstract void ElementRead(NetDataReader r, out TProperty prop);
            protected abstract void ElementWrite(NetDataWriter w, ref TProperty prop);

            public override void Read(TClass inf, NetDataReader r)
            {
                TProperty elem;
                ElementRead(r, out elem);
                Setter(inf, elem);
            }

            public override void Write(TClass inf, NetDataWriter w)
            {
                var elem = Getter(inf);
                ElementWrite(w, ref elem);
            }

            public override void ReadArray(TClass inf, NetDataReader r)
            {
                var arr = ReadArrayHelper(inf, r);
                for (int i = 0; i < arr.Length; i++)
                    ElementRead(r, out arr[i]);
            }

            public override void WriteArray(TClass inf, NetDataWriter w)
            {
                var arr = WriteArrayHelper(inf, w);
                for (int i = 0; i < arr.Length; i++)
                    ElementWrite(w, ref arr[i]);
            }
        }

        private sealed class FastCallStatic<TClass, TProperty> : FastCallSpecific<TClass, TProperty>
        {
            private readonly Action<NetDataWriter, TProperty> _writer;
            private readonly Func<NetDataReader, TProperty> _reader;

            public FastCallStatic(Action<NetDataWriter, TProperty> write, Func<NetDataReader, TProperty> read)
            {
                _writer = write;
                _reader = read;
            }

            public override void Read(TClass inf, NetDataReader r) { Setter(inf, _reader(r)); }
            public override void Write(TClass inf, NetDataWriter w) { _writer(w, Getter(inf)); }

            public override void ReadList(TClass inf, NetDataReader r)
            {
                int len;
                var list = ReadListHelper(inf, r, out len);
                int listCount = list.Count;
                for (int i = 0; i < len; i++)
                {
                    if (i < listCount)
                        list[i] = _reader(r);
                    else
                        list.Add(_reader(r));
                }
                if (len < listCount)
                    list.RemoveRange(len, listCount - len);
            }

            public override void WriteList(TClass inf, NetDataWriter w)
            {
                int len;
                var list = WriteListHelper(inf, w, out len);
                for (int i = 0; i < len; i++)
                    _writer(w, list[i]);
            }

            public override void ReadArray(TClass inf, NetDataReader r)
            {
                var arr = ReadArrayHelper(inf, r);
                int len = arr.Length;
                for (int i = 0; i < len; i++)
                    arr[i] = _reader(r);
            }

            public override void WriteArray(TClass inf, NetDataWriter w)
            {
                var arr = WriteArrayHelper(inf, w);
                int len = arr.Length;
                for (int i = 0; i < len; i++)
                    _writer(w, arr[i]);
            }
        }

        private sealed class FastCallStruct<TClass, TProperty> : FastCallSpecific<TClass, TProperty> where TProperty : struct, INetSerializable
        {
            private TProperty _p;

            public override void Read(TClass inf, NetDataReader r)
            {
                _p.Deserialize(r);
                Setter(inf, _p);
            }

            public override void Write(TClass inf, NetDataWriter w)
            {
                _p = Getter(inf);
                _p.Serialize(w);
            }

            public override void ReadList(TClass inf, NetDataReader r)
            {
                int len;
                var list = ReadListHelper(inf, r, out len);
                int listCount = list.Count;
                for (int i = 0; i < len; i++)
                {
                    var itm = default(TProperty);
                    itm.Deserialize(r);
                    if(i < listCount)
                        list[i] = itm;
                    else
                        list.Add(itm);
                }
                if (len < listCount)
                    list.RemoveRange(len, listCount - len);
            }

            public override void WriteList(TClass inf, NetDataWriter w)
            {
                int len;
                var list = WriteListHelper(inf, w, out len);
                for (int i = 0; i < len; i++)
                    list[i].Serialize(w);
            }

            public override void ReadArray(TClass inf, NetDataReader r)
            {
                var arr = ReadArrayHelper(inf, r);
                int len = arr.Length;
                for (int i = 0; i < len; i++)
                    arr[i].Deserialize(r);
            }

            public override void WriteArray(TClass inf, NetDataWriter w)
            {
                var arr = WriteArrayHelper(inf, w);
                int len = arr.Length;
                for (int i = 0; i < len; i++)
                    arr[i].Serialize(w);
            }
        }

        private sealed class FastCallClass<TClass, TProperty> : FastCallSpecific<TClass, TProperty> where TProperty : class, INetSerializable
        {
            private readonly Func<TProperty> _constructor;
            public FastCallClass(Func<TProperty> constructor) { _constructor = constructor; }

            public override void Read(TClass inf, NetDataReader r)
            {
                var p = _constructor();
                p.Deserialize(r);
                Setter(inf, p);
            }

            public override void Write(TClass inf, NetDataWriter w)
            {
                var p = Getter(inf);
                if(p != null)
                    p.Serialize(w);
            }

            public override void ReadList(TClass inf, NetDataReader r)
            {
                int len;
                var list = ReadListHelper(inf, r, out len);
                int listCount = list.Count;
                for (int i = 0; i < len; i++)
                {
                    if (i < listCount)
                    {
                        list[i].Deserialize(r);
                    }
                    else
                    {
                        var itm = _constructor();
                        itm.Deserialize(r);
                        list.Add(itm);
                    }
                }
                if (len < listCount)
                    list.RemoveRange(len, listCount - len);
            }

            public override void WriteList(TClass inf, NetDataWriter w)
            {
                int len;
                var list = WriteListHelper(inf, w, out len);
                for (int i = 0; i < len; i++)
                    list[i].Serialize(w);
            }

            public override void ReadArray(TClass inf, NetDataReader r)
            {
                var arr = ReadArrayHelper(inf, r);
                int len = arr.Length;
                for (int i = 0; i < len; i++)
                {
                    arr[i] = _constructor();
                    arr[i].Deserialize(r);
                }
            }

            public override void WriteArray(TClass inf, NetDataWriter w)
            {
                var arr = WriteArrayHelper(inf, w);
                int len = arr.Length;
                for (int i = 0; i < len; i++)
                    arr[i].Serialize(w);
            }
        }

        private class IntSerializer<T> : FastCallSpecific<T, int>
        {
            public override void Read(T inf, NetDataReader r) { Setter(inf, r.GetInt()); }
            public override void Write(T inf, NetDataWriter w) { w.Put(Getter(inf)); }
            public override void ReadArray(T inf, NetDataReader r) { SetterArr(inf, r.GetIntArray()); }
            public override void WriteArray(T inf, NetDataWriter w) { w.PutArray(GetterArr(inf)); }
        }

        private class UIntSerializer<T> : FastCallSpecific<T, uint>
        {
            public override void Read(T inf, NetDataReader r) { Setter(inf, r.GetUInt()); }
            public override void Write(T inf, NetDataWriter w) { w.Put(Getter(inf)); }
            public override void ReadArray(T inf, NetDataReader r) { SetterArr(inf, r.GetUIntArray()); }
            public override void WriteArray(T inf, NetDataWriter w) { w.PutArray(GetterArr(inf)); }
        }

        private class ShortSerializer<T> : FastCallSpecific<T, short>
        {
            public override void Read(T inf, NetDataReader r) { Setter(inf, r.GetShort()); }
            public override void Write(T inf, NetDataWriter w) { w.Put(Getter(inf)); }
            public override void ReadArray(T inf, NetDataReader r) { SetterArr(inf, r.GetShortArray()); }
            public override void WriteArray(T inf, NetDataWriter w) { w.PutArray(GetterArr(inf)); }
        }

        private class UShortSerializer<T> : FastCallSpecific<T, ushort>
        {
            public override void Read(T inf, NetDataReader r) { Setter(inf, r.GetUShort()); }
            public override void Write(T inf, NetDataWriter w) { w.Put(Getter(inf)); }
            public override void ReadArray(T inf, NetDataReader r) { SetterArr(inf, r.GetUShortArray()); }
            public override void WriteArray(T inf, NetDataWriter w) { w.PutArray(GetterArr(inf)); }
        }

        private class LongSerializer<T> : FastCallSpecific<T, long>
        {
            public override void Read(T inf, NetDataReader r) { Setter(inf, r.GetLong()); }
            public override void Write(T inf, NetDataWriter w) { w.Put(Getter(inf)); }
            public override void ReadArray(T inf, NetDataReader r) { SetterArr(inf, r.GetLongArray()); }
            public override void WriteArray(T inf, NetDataWriter w) { w.PutArray(GetterArr(inf)); }
        }

        private class ULongSerializer<T> : FastCallSpecific<T, ulong>
        {
            public override void Read(T inf, NetDataReader r) { Setter(inf, r.GetULong()); }
            public override void Write(T inf, NetDataWriter w) { w.Put(Getter(inf)); }
            public override void ReadArray(T inf, NetDataReader r) { SetterArr(inf, r.GetULongArray()); }
            public override void WriteArray(T inf, NetDataWriter w) { w.PutArray(GetterArr(inf)); }
        }

        private class ByteSerializer<T> : FastCallSpecific<T, byte>
        {
            public override void Read(T inf, NetDataReader r) { Setter(inf, r.GetByte()); }
            public override void Write(T inf, NetDataWriter w) { w.Put(Getter(inf)); }
            public override void ReadArray(T inf, NetDataReader r) { SetterArr(inf, r.GetBytesWithLength()); }
            public override void WriteArray(T inf, NetDataWriter w) { w.PutBytesWithLength(GetterArr(inf)); }
        }

        private class SByteSerializer<T> : FastCallSpecific<T, sbyte>
        {
            public override void Read(T inf, NetDataReader r) { Setter(inf, r.GetSByte()); }
            public override void Write(T inf, NetDataWriter w) { w.Put(Getter(inf)); }
            public override void ReadArray(T inf, NetDataReader r) { SetterArr(inf, r.GetSBytesWithLength()); }
            public override void WriteArray(T inf, NetDataWriter w) { w.PutSBytesWithLength(GetterArr(inf)); }
        }

        private class FloatSerializer<T> : FastCallSpecific<T, float>
        {
            public override void Read(T inf, NetDataReader r) { Setter(inf, r.GetFloat()); }
            public override void Write(T inf, NetDataWriter w) { w.Put(Getter(inf)); }
            public override void ReadArray(T inf, NetDataReader r) { SetterArr(inf, r.GetFloatArray()); }
            public override void WriteArray(T inf, NetDataWriter w) { w.PutArray(GetterArr(inf)); }
        }

        private class DoubleSerializer<T> : FastCallSpecific<T, double>
        {
            public override void Read(T inf, NetDataReader r) { Setter(inf, r.GetDouble()); }
            public override void Write(T inf, NetDataWriter w) { w.Put(Getter(inf)); }
            public override void ReadArray(T inf, NetDataReader r) { SetterArr(inf, r.GetDoubleArray()); }
            public override void WriteArray(T inf, NetDataWriter w) { w.PutArray(GetterArr(inf)); }
        }

        private class BoolSerializer<T> : FastCallSpecific<T, bool>
        {
            public override void Read(T inf, NetDataReader r) { Setter(inf, r.GetBool()); }
            public override void Write(T inf, NetDataWriter w) { w.Put(Getter(inf)); }
            public override void ReadArray(T inf, NetDataReader r) { SetterArr(inf, r.GetBoolArray()); }
            public override void WriteArray(T inf, NetDataWriter w) { w.PutArray(GetterArr(inf)); }
        }

        private class CharSerializer<T> : FastCallSpecificAuto<T, char>
        {
            protected override void ElementWrite(NetDataWriter w, ref char prop) { w.Put(prop); }
            protected override void ElementRead(NetDataReader r, out char prop) { prop = r.GetChar(); }
        }

        private class IPEndPointSerializer<T> : FastCallSpecificAuto<T, IPEndPoint>
        {
            protected override void ElementWrite(NetDataWriter w, ref IPEndPoint prop) { w.Put(prop); }
            protected override void ElementRead(NetDataReader r, out IPEndPoint prop) { prop = r.GetNetEndPoint(); }
        }

        private class StringSerializer<T> : FastCallSpecific<T, string>
        {
            private readonly int _maxLength;
            public StringSerializer(int maxLength) { _maxLength = maxLength > 0 ? maxLength : short.MaxValue; }
            public override void Read(T inf, NetDataReader r) { Setter(inf, r.GetString(_maxLength)); }
            public override void Write(T inf, NetDataWriter w) { w.Put(Getter(inf), _maxLength); }
            public override void ReadArray(T inf, NetDataReader r) { SetterArr(inf, r.GetStringArray(_maxLength)); }
            public override void WriteArray(T inf, NetDataWriter w) { w.PutArray(GetterArr(inf), _maxLength); }
        }

        private class EnumByteSerializer<T> : FastCall<T>
        {
            protected readonly PropertyInfo Property;
            protected readonly Type PropertyType;
            public EnumByteSerializer(PropertyInfo property, Type propertyType)
            {
                Property = property;
                PropertyType = propertyType;
            }
            public override void Read(T inf, NetDataReader r) { Property.SetValue(inf, Enum.ToObject(PropertyType, r.GetByte()), null); }
            public override void Write(T inf, NetDataWriter w) { w.Put((byte)Property.GetValue(inf, null)); }
            public override void ReadArray(T inf, NetDataReader r) { throw new InvalidTypeException("Unsupported type: Enum[]"); }
            public override void WriteArray(T inf, NetDataWriter w) { throw new InvalidTypeException("Unsupported type: Enum[]"); }
            public override void ReadList(T inf, NetDataReader r) { throw new InvalidTypeException("Unsupported type: List<Enum>"); }
            public override void WriteList(T inf, NetDataWriter w) { throw new InvalidTypeException("Unsupported type: List<Enum>"); }
        }

        private class EnumIntSerializer<T> : EnumByteSerializer<T>
        {
            public EnumIntSerializer(PropertyInfo property, Type propertyType) : base(property, propertyType) { }
            public override void Read(T inf, NetDataReader r) { Property.SetValue(inf, Enum.ToObject(PropertyType, r.GetInt()), null); }
            public override void Write(T inf, NetDataWriter w) { w.Put((int)Property.GetValue(inf, null)); }
        }

        private sealed class ClassInfo<T>
        {
            public static ClassInfo<T> Instance;
            private readonly FastCall<T>[] _serializers;
            private readonly int _membersCount;

            public ClassInfo(List<FastCall<T>> serializers)
            {
                _membersCount = serializers.Count;
                _serializers = serializers.ToArray();
            }

            public void Write(T obj, NetDataWriter writer)
            {
                for (int i = 0; i < _membersCount; i++)
                {
                    var s = _serializers[i];
                    if (s.Type == CallType.Basic)
                        s.Write(obj, writer);
                    else if (s.Type == CallType.Array)
                        s.WriteArray(obj, writer);
                    else
                        s.WriteList(obj, writer);
                }
            }

            public void Read(T obj, NetDataReader reader)
            {
                for (int i = 0; i < _membersCount; i++)
                {
                    var s = _serializers[i];
                    if (s.Type == CallType.Basic)
                        s.Read(obj, reader);
                    else if(s.Type == CallType.Array)
                        s.ReadArray(obj, reader);
                    else
                        s.ReadList(obj, reader);
                }
            }
        }

        private abstract class CustomType
        {
            public abstract FastCall<T> Get<T>();
        }

        private sealed class CustomTypeStruct<TProperty> : CustomType where TProperty : struct, INetSerializable
        {
            public override FastCall<T> Get<T>() { return new FastCallStruct<T, TProperty>(); }
        }

        private sealed class CustomTypeClass<TProperty> : CustomType where TProperty : class, INetSerializable
        {
            private readonly Func<TProperty> _constructor;
            public CustomTypeClass(Func<TProperty> constructor) { _constructor = constructor; }
            public override FastCall<T> Get<T>() { return new FastCallClass<T, TProperty>(_constructor); }
        }

        private sealed class CustomTypeStatic<TProperty> : CustomType
        {
            private readonly Action<NetDataWriter, TProperty> _writer;
            private readonly Func<NetDataReader, TProperty> _reader;
            public CustomTypeStatic(Action<NetDataWriter, TProperty> writer, Func<NetDataReader, TProperty> reader)
            {
                _writer = writer;
                _reader = reader;
            }
            public override FastCall<T> Get<T>() { return new FastCallStatic<T, TProperty>(_writer, _reader); }
        }

        /// <summary>
        /// Register custom property type
        /// </summary>
        /// <typeparam name="T">INetSerializable structure</typeparam>
        public void RegisterNestedType<T>() where T : struct, INetSerializable
        {
            _registeredTypes.Add(typeof(T), new CustomTypeStruct<T>());
        }

        /// <summary>
        /// Register custom property type
        /// </summary>
        /// <typeparam name="T">INetSerializable class</typeparam>
        public void RegisterNestedType<T>(Func<T> constructor) where T : class, INetSerializable
        {
            _registeredTypes.Add(typeof(T), new CustomTypeClass<T>(constructor));
        }

        /// <summary>
        /// Register custom property type
        /// </summary>
        /// <typeparam name="T">Any packet</typeparam>
        /// <param name="writer">custom type writer</param>
        /// <param name="reader">custom type reader</param>
        public void RegisterNestedType<T>(Action<NetDataWriter, T> writer, Func<NetDataReader, T> reader)
        {
            _registeredTypes.Add(typeof(T), new CustomTypeStatic<T>(writer, reader));
        }

        private NetDataWriter _writer;
        private readonly int _maxStringLength;
        private readonly Dictionary<Type, CustomType> _registeredTypes = new Dictionary<Type, CustomType>();

        public NetSerializer() : this(0)
        {
        }

        public NetSerializer(int maxStringLength)
        {
            _maxStringLength = maxStringLength;
        }

        private ClassInfo<T> RegisterInternal<T>()
        {
            if (ClassInfo<T>.Instance != null)
                return ClassInfo<T>.Instance;

            Type t = typeof(T);
            var props = t.GetProperties(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.GetProperty |
                BindingFlags.SetProperty);
            var serializers = new List<FastCall<T>>();
            for (int i = 0; i < props.Length; i++)
            {
                var property = props[i];
                var propertyType = property.PropertyType;

                var elementType = propertyType.IsArray ? propertyType.GetElementType() : propertyType;
                var callType = propertyType.IsArray ? CallType.Array : CallType.Basic;

                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    elementType = propertyType.GetGenericArguments()[0];
                    callType = CallType.List;
                }
                
                // if (Attribute.IsDefined(property, typeof(IgnoreDataMemberAttribute)))
                    // continue;

                var getMethod = property.GetGetMethod();
                var setMethod = property.GetSetMethod();
                if (getMethod == null || setMethod == null)
                    continue;

                FastCall<T> serialzer = null;
                if (propertyType.IsEnum)
                {
                    var underlyingType = Enum.GetUnderlyingType(propertyType);
                    if (underlyingType == typeof(byte))
                        serialzer = new EnumByteSerializer<T>(property, propertyType);
                    else if (underlyingType == typeof(int))
                        serialzer = new EnumIntSerializer<T>(property, propertyType);
                    else
                        throw new InvalidTypeException("Not supported enum underlying type: " + underlyingType.Name);
                }
                else if (elementType == typeof(string))
                    serialzer = new StringSerializer<T>(_maxStringLength);
                else if (elementType == typeof(bool))
                    serialzer = new BoolSerializer<T>();
                else if (elementType == typeof(byte))
                    serialzer = new ByteSerializer<T>();
                else if (elementType == typeof(sbyte))
                    serialzer = new SByteSerializer<T>();
                else if (elementType == typeof(short))
                    serialzer = new ShortSerializer<T>();
                else if (elementType == typeof(ushort))
                    serialzer = new UShortSerializer<T>();
                else if (elementType == typeof(int))
                    serialzer = new IntSerializer<T>();
                else if (elementType == typeof(uint))
                    serialzer = new UIntSerializer<T>();
                else if (elementType == typeof(long))
                    serialzer = new LongSerializer<T>();
                else if (elementType == typeof(ulong))
                    serialzer = new ULongSerializer<T>();
                else if (elementType == typeof(float))
                    serialzer = new FloatSerializer<T>();
                else if (elementType == typeof(double))
                    serialzer = new DoubleSerializer<T>();
                else if (elementType == typeof(char))
                    serialzer = new CharSerializer<T>();
                else if (elementType == typeof(IPEndPoint))
                    serialzer = new IPEndPointSerializer<T>();
                else
                {
                    CustomType customType;
                    _registeredTypes.TryGetValue(elementType, out customType);
                    if (customType != null)
                        serialzer = customType.Get<T>();
                }

                if (serialzer != null)
                {
                    serialzer.Init(getMethod, setMethod, callType);
                    serializers.Add(serialzer);
                }
                else
                {
                    throw new InvalidTypeException("Unknown property type: " + propertyType.FullName);
                }
            }
            ClassInfo<T>.Instance = new ClassInfo<T>(serializers);
            return ClassInfo<T>.Instance;
        }

        /// <exception cref="InvalidTypeException"><typeparamref name="T"/>'s fields are not supported, or it has no fields</exception>
        public void Register<T>()
        {
            RegisterInternal<T>();
        }

        /// <summary>
        /// Reads packet with known type
        /// </summary>
        /// <param name="reader">NetDataReader with packet</param>
        /// <returns>Returns packet if packet in reader is matched type</returns>
        /// <exception cref="InvalidTypeException"><typeparamref name="T"/>'s fields are not supported, or it has no fields</exception>
        public T Deserialize<T>(NetDataReader reader) where T : class, new()
        {
            var info = RegisterInternal<T>();
            var result = new T();
            try
            {
                info.Read(result, reader);
            }
            catch
            {
                return null;
            }
            return result;
        }

        /// <summary>
        /// Reads packet with known type (non alloc variant)
        /// </summary>
        /// <param name="reader">NetDataReader with packet</param>
        /// <param name="target">Deserialization target</param>
        /// <returns>Returns true if packet in reader is matched type</returns>
        /// <exception cref="InvalidTypeException"><typeparamref name="T"/>'s fields are not supported, or it has no fields</exception>
        public bool Deserialize<T>(NetDataReader reader, T target) where T : class, new()
        {
            var info = RegisterInternal<T>();
            try
            {
                info.Read(target, reader);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Serialize object to NetDataWriter (fast)
        /// </summary>
        /// <param name="writer">Serialization target NetDataWriter</param>
        /// <param name="obj">Object to serialize</param>
        /// <exception cref="InvalidTypeException"><typeparamref name="T"/>'s fields are not supported, or it has no fields</exception>
        public void Serialize<T>(NetDataWriter writer, T obj) where T : class, new()
        {
            RegisterInternal<T>().Write(obj, writer);
        }

        /// <summary>
        /// Serialize object to byte array
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>byte array with serialized data</returns>
        public byte[] Serialize<T>(T obj) where T : class, new()
        {
            if (_writer == null)
                _writer = new NetDataWriter();
            _writer.Reset();
            Serialize(_writer, obj);
            return _writer.CopyData();
        }
    }
}
