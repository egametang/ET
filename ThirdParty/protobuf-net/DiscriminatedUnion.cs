using System;
using System.Runtime.InteropServices;

namespace ProtoBuf
{
    /// <summary>Represent multiple types as a union; this is used as part of OneOf -
    /// note that it is the caller's responsbility to only read/write the value as the same type</summary>
    public readonly partial struct DiscriminatedUnionObject
    {

        /// <summary>The value typed as Object</summary>
        public readonly object Object;

        /// <summary>Indicates whether the specified discriminator is assigned</summary>
        public bool Is(int discriminator) => Discriminator == discriminator;

        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnionObject(int discriminator, object value)
        {
            Discriminator = discriminator;
            Object = value;
        }

        /// <summary>Reset a value if the specified discriminator is assigned</summary>
        public static void Reset(ref DiscriminatedUnionObject value, int discriminator)
        {
            if (value.Discriminator == discriminator) value = default;
        }

        /// <summary>The discriminator value</summary>
        public int Discriminator { get; }
    }

    /// <summary>Represent multiple types as a union; this is used as part of OneOf -
    /// note that it is the caller's responsbility to only read/write the value as the same type</summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly partial struct DiscriminatedUnion64
    {
#if !FEAT_SAFE
		unsafe static DiscriminatedUnion64()
        {
            if (sizeof(DateTime) > 8) throw new InvalidOperationException(nameof(DateTime) + " was unexpectedly too big for " + nameof(DiscriminatedUnion64));
            if (sizeof(TimeSpan) > 8) throw new InvalidOperationException(nameof(TimeSpan) + " was unexpectedly too big for " + nameof(DiscriminatedUnion64));
        }
#endif
		[FieldOffset(0)] private readonly int _discriminator;  // note that we can't pack further because Object needs x8 alignment/padding on x64

        /// <summary>The value typed as Int64</summary>
        [FieldOffset(8)] public readonly long Int64;
        /// <summary>The value typed as UInt64</summary>
        [FieldOffset(8)] public readonly ulong UInt64;
        /// <summary>The value typed as Int32</summary>
        [FieldOffset(8)] public readonly int Int32;
        /// <summary>The value typed as UInt32</summary>
        [FieldOffset(8)] public readonly uint UInt32;
        /// <summary>The value typed as Boolean</summary>
        [FieldOffset(8)] public readonly bool Boolean;
        /// <summary>The value typed as Single</summary>
        [FieldOffset(8)] public readonly float Single;
        /// <summary>The value typed as Double</summary>
        [FieldOffset(8)] public readonly double Double;
        /// <summary>The value typed as DateTime</summary>
        [FieldOffset(8)] public readonly DateTime DateTime;
        /// <summary>The value typed as TimeSpan</summary>
        [FieldOffset(8)] public readonly TimeSpan TimeSpan;

        private DiscriminatedUnion64(int discriminator) : this()
        {
            _discriminator = discriminator;
        }

        /// <summary>Indicates whether the specified discriminator is assigned</summary>
        public bool Is(int discriminator) => _discriminator == discriminator;

        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion64(int discriminator, long value) : this(discriminator) { Int64 = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion64(int discriminator, int value) : this(discriminator) { Int32 = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion64(int discriminator, ulong value) : this(discriminator) { UInt64 = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion64(int discriminator, uint value) : this(discriminator) { UInt32 = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion64(int discriminator, float value) : this(discriminator) { Single = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion64(int discriminator, double value) : this(discriminator) { Double = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion64(int discriminator, bool value) : this(discriminator) { Boolean = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion64(int discriminator, DateTime? value) : this(value.HasValue ? discriminator: 0) { DateTime = value.GetValueOrDefault(); }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion64(int discriminator, TimeSpan? value) : this(value.HasValue ? discriminator : 0) { TimeSpan = value.GetValueOrDefault(); }

        /// <summary>Reset a value if the specified discriminator is assigned</summary>
        public static void Reset(ref DiscriminatedUnion64 value, int discriminator)
        {
            if (value.Discriminator == discriminator) value = default;
        }
        /// <summary>The discriminator value</summary>
        public int Discriminator => _discriminator;
    }

    /// <summary>Represent multiple types as a union; this is used as part of OneOf -
    /// note that it is the caller's responsbility to only read/write the value as the same type</summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly partial struct DiscriminatedUnion128Object
    {
#if !FEAT_SAFE
		unsafe static DiscriminatedUnion128Object()
        {
            if (sizeof(DateTime) > 16) throw new InvalidOperationException(nameof(DateTime) + " was unexpectedly too big for " + nameof(DiscriminatedUnion128Object));
            if (sizeof(TimeSpan) > 16) throw new InvalidOperationException(nameof(TimeSpan) + " was unexpectedly too big for " + nameof(DiscriminatedUnion128Object));
            if (sizeof(Guid) > 16) throw new InvalidOperationException(nameof(Guid) + " was unexpectedly too big for " + nameof(DiscriminatedUnion128Object));
        }
#endif

		[FieldOffset(0)] private readonly int _discriminator;  // note that we can't pack further because Object needs x8 alignment/padding on x64

        /// <summary>The value typed as Int64</summary>
        [FieldOffset(8)] public readonly long Int64;
        /// <summary>The value typed as UInt64</summary>
        [FieldOffset(8)] public readonly ulong UInt64;
        /// <summary>The value typed as Int32</summary>
        [FieldOffset(8)] public readonly int Int32;
        /// <summary>The value typed as UInt32</summary>
        [FieldOffset(8)] public readonly uint UInt32;
        /// <summary>The value typed as Boolean</summary>
        [FieldOffset(8)] public readonly bool Boolean;
        /// <summary>The value typed as Single</summary>
        [FieldOffset(8)] public readonly float Single;
        /// <summary>The value typed as Double</summary>
        [FieldOffset(8)] public readonly double Double;
        /// <summary>The value typed as DateTime</summary>
        [FieldOffset(8)] public readonly DateTime DateTime;
        /// <summary>The value typed as TimeSpan</summary>
        [FieldOffset(8)] public readonly TimeSpan TimeSpan;
        /// <summary>The value typed as Guid</summary>
        [FieldOffset(8)] public readonly Guid Guid;
        /// <summary>The value typed as Object</summary>
        [FieldOffset(24)] public readonly object Object;

        private DiscriminatedUnion128Object(int discriminator) : this()
        {
            _discriminator = discriminator;
        }

        /// <summary>Indicates whether the specified discriminator is assigned</summary>
        public bool Is(int discriminator) => _discriminator == discriminator;

        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128Object(int discriminator, long value) : this(discriminator) { Int64 = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128Object(int discriminator, int value) : this(discriminator) { Int32 = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128Object(int discriminator, ulong value) : this(discriminator) { UInt64 = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128Object(int discriminator, uint value) : this(discriminator) { UInt32 = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128Object(int discriminator, float value) : this(discriminator) { Single = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128Object(int discriminator, double value) : this(discriminator) { Double = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128Object(int discriminator, bool value) : this(discriminator) { Boolean = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128Object(int discriminator, object value) : this(value != null ? discriminator : 0) { Object = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128Object(int discriminator, DateTime? value) : this(value.HasValue ? discriminator: 0) { DateTime = value.GetValueOrDefault(); }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128Object(int discriminator, TimeSpan? value) : this(value.HasValue ? discriminator : 0) { TimeSpan = value.GetValueOrDefault(); }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128Object(int discriminator, Guid? value) : this(value.HasValue ? discriminator : 0) { Guid = value.GetValueOrDefault(); }

        /// <summary>Reset a value if the specified discriminator is assigned</summary>
        public static void Reset(ref DiscriminatedUnion128Object value, int discriminator)
        {
            if (value.Discriminator == discriminator) value = default;
        }
        /// <summary>The discriminator value</summary>
        public int Discriminator => _discriminator;
    }

    /// <summary>Represent multiple types as a union; this is used as part of OneOf -
    /// note that it is the caller's responsbility to only read/write the value as the same type</summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly partial struct DiscriminatedUnion128
    {
#if !FEAT_SAFE
        unsafe static DiscriminatedUnion128()
        {
            if (sizeof(DateTime) > 16) throw new InvalidOperationException(nameof(DateTime) + " was unexpectedly too big for " + nameof(DiscriminatedUnion128));
            if (sizeof(TimeSpan) > 16) throw new InvalidOperationException(nameof(TimeSpan) + " was unexpectedly too big for " + nameof(DiscriminatedUnion128));
            if (sizeof(Guid) > 16) throw new InvalidOperationException(nameof(Guid) + " was unexpectedly too big for " + nameof(DiscriminatedUnion128));
        }
#endif
        [FieldOffset(0)] private readonly int _discriminator;  // note that we can't pack further because Object needs x8 alignment/padding on x64

        /// <summary>The value typed as Int64</summary>
        [FieldOffset(8)] public readonly long Int64;
        /// <summary>The value typed as UInt64</summary>
        [FieldOffset(8)] public readonly ulong UInt64;
        /// <summary>The value typed as Int32</summary>
        [FieldOffset(8)] public readonly int Int32;
        /// <summary>The value typed as UInt32</summary>
        [FieldOffset(8)] public readonly uint UInt32;
        /// <summary>The value typed as Boolean</summary>
        [FieldOffset(8)] public readonly bool Boolean;
        /// <summary>The value typed as Single</summary>
        [FieldOffset(8)] public readonly float Single;
        /// <summary>The value typed as Double</summary>
        [FieldOffset(8)] public readonly double Double;
        /// <summary>The value typed as DateTime</summary>
        [FieldOffset(8)] public readonly DateTime DateTime;
        /// <summary>The value typed as TimeSpan</summary>
        [FieldOffset(8)] public readonly TimeSpan TimeSpan;
        /// <summary>The value typed as Guid</summary>
        [FieldOffset(8)] public readonly Guid Guid;

        private DiscriminatedUnion128(int discriminator) : this()
        {
            _discriminator = discriminator;
        }

        /// <summary>Indicates whether the specified discriminator is assigned</summary>
        public bool Is(int discriminator) => _discriminator == discriminator;

        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128(int discriminator, long value) : this(discriminator) { Int64 = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128(int discriminator, int value) : this(discriminator) { Int32 = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128(int discriminator, ulong value) : this(discriminator) { UInt64 = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128(int discriminator, uint value) : this(discriminator) { UInt32 = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128(int discriminator, float value) : this(discriminator) { Single = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128(int discriminator, double value) : this(discriminator) { Double = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128(int discriminator, bool value) : this(discriminator) { Boolean = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128(int discriminator, DateTime? value) : this(value.HasValue ? discriminator: 0) { DateTime = value.GetValueOrDefault(); }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128(int discriminator, TimeSpan? value) : this(value.HasValue ? discriminator : 0) { TimeSpan = value.GetValueOrDefault(); }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion128(int discriminator, Guid? value) : this(value.HasValue ? discriminator : 0) { Guid = value.GetValueOrDefault(); }

        /// <summary>Reset a value if the specified discriminator is assigned</summary>
        public static void Reset(ref DiscriminatedUnion128 value, int discriminator)
        {
            if (value.Discriminator == discriminator) value = default;
        }
        /// <summary>The discriminator value</summary>
        public int Discriminator => _discriminator;
    }

    /// <summary>Represent multiple types as a union; this is used as part of OneOf -
    /// note that it is the caller's responsbility to only read/write the value as the same type</summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly partial struct DiscriminatedUnion64Object
    {
#if !FEAT_SAFE
        unsafe static DiscriminatedUnion64Object()
        {
            if (sizeof(DateTime) > 8) throw new InvalidOperationException(nameof(DateTime) + " was unexpectedly too big for " + nameof(DiscriminatedUnion64Object));
            if (sizeof(TimeSpan) > 8) throw new InvalidOperationException(nameof(TimeSpan) + " was unexpectedly too big for " + nameof(DiscriminatedUnion64Object));
        }
#endif
        [FieldOffset(0)] private readonly int _discriminator;  // note that we can't pack further because Object needs x8 alignment/padding on x64

        /// <summary>The value typed as Int64</summary>
        [FieldOffset(8)] public readonly long Int64;
        /// <summary>The value typed as UInt64</summary>
        [FieldOffset(8)] public readonly ulong UInt64;
        /// <summary>The value typed as Int32</summary>
        [FieldOffset(8)] public readonly int Int32;
        /// <summary>The value typed as UInt32</summary>
        [FieldOffset(8)] public readonly uint UInt32;
        /// <summary>The value typed as Boolean</summary>
        [FieldOffset(8)] public readonly bool Boolean;
        /// <summary>The value typed as Single</summary>
        [FieldOffset(8)] public readonly float Single;
        /// <summary>The value typed as Double</summary>
        [FieldOffset(8)] public readonly double Double;
        /// <summary>The value typed as DateTime</summary>
        [FieldOffset(8)] public readonly DateTime DateTime;
        /// <summary>The value typed as TimeSpan</summary>
        [FieldOffset(8)] public readonly TimeSpan TimeSpan;
        /// <summary>The value typed as Object</summary>
        [FieldOffset(16)] public readonly object Object;

        private DiscriminatedUnion64Object(int discriminator) : this()
        {
            _discriminator = discriminator;
        }

        /// <summary>Indicates whether the specified discriminator is assigned</summary>
        public bool Is(int discriminator) => _discriminator == discriminator;

        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion64Object(int discriminator, long value) : this(discriminator) { Int64 = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion64Object(int discriminator, int value) : this(discriminator) { Int32 = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion64Object(int discriminator, ulong value) : this(discriminator) { UInt64 = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion64Object(int discriminator, uint value) : this(discriminator) { UInt32 = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion64Object(int discriminator, float value) : this(discriminator) { Single = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion64Object(int discriminator, double value) : this(discriminator) { Double = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion64Object(int discriminator, bool value) : this(discriminator) { Boolean = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion64Object(int discriminator, object value) : this(value != null ? discriminator : 0) { Object = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion64Object(int discriminator, DateTime? value) : this(value.HasValue ? discriminator: 0) { DateTime = value.GetValueOrDefault(); }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion64Object(int discriminator, TimeSpan? value) : this(value.HasValue ? discriminator : 0) { TimeSpan = value.GetValueOrDefault(); }

        /// <summary>Reset a value if the specified discriminator is assigned</summary>
        public static void Reset(ref DiscriminatedUnion64Object value, int discriminator)
        {
            if (value.Discriminator == discriminator) value = default;
        }
        /// <summary>The discriminator value</summary>
        public int Discriminator => _discriminator;
    }

    /// <summary>Represent multiple types as a union; this is used as part of OneOf -
    /// note that it is the caller's responsbility to only read/write the value as the same type</summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly partial struct DiscriminatedUnion32
    {
        [FieldOffset(0)] private readonly int _discriminator;

        /// <summary>The value typed as Int32</summary>
        [FieldOffset(4)] public readonly int Int32;
        /// <summary>The value typed as UInt32</summary>
        [FieldOffset(4)] public readonly uint UInt32;
        /// <summary>The value typed as Boolean</summary>
        [FieldOffset(4)] public readonly bool Boolean;
        /// <summary>The value typed as Single</summary>
        [FieldOffset(4)] public readonly float Single;

        private DiscriminatedUnion32(int discriminator) : this()
        {
            _discriminator = discriminator;
        }

        /// <summary>Indicates whether the specified discriminator is assigned</summary>
        public bool Is(int discriminator) => _discriminator == discriminator;

        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion32(int discriminator, int value) : this(discriminator) { Int32 = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion32(int discriminator, uint value) : this(discriminator) { UInt32 = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion32(int discriminator, float value) : this(discriminator) { Single = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion32(int discriminator, bool value) : this(discriminator) { Boolean = value; }

        /// <summary>Reset a value if the specified discriminator is assigned</summary>
        public static void Reset(ref DiscriminatedUnion32 value, int discriminator)
        {
            if (value.Discriminator == discriminator) value = default;
        }
        /// <summary>The discriminator value</summary>
        public int Discriminator => _discriminator;
    }

    /// <summary>Represent multiple types as a union; this is used as part of OneOf -
    /// note that it is the caller's responsbility to only read/write the value as the same type</summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly partial struct DiscriminatedUnion32Object
    {
        [FieldOffset(0)] private readonly int _discriminator;

        /// <summary>The value typed as Int32</summary>
        [FieldOffset(4)] public readonly int Int32;
        /// <summary>The value typed as UInt32</summary>
        [FieldOffset(4)] public readonly uint UInt32;
        /// <summary>The value typed as Boolean</summary>
        [FieldOffset(4)] public readonly bool Boolean;
        /// <summary>The value typed as Single</summary>
        [FieldOffset(4)] public readonly float Single;
        /// <summary>The value typed as Object</summary>
        [FieldOffset(8)] public readonly object Object;

        private DiscriminatedUnion32Object(int discriminator) : this()
        {
            _discriminator = discriminator;
        }

        /// <summary>Indicates whether the specified discriminator is assigned</summary>
        public bool Is(int discriminator) => _discriminator == discriminator;

        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion32Object(int discriminator, int value) : this(discriminator) { Int32 = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion32Object(int discriminator, uint value) : this(discriminator) { UInt32 = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion32Object(int discriminator, float value) : this(discriminator) { Single = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion32Object(int discriminator, bool value) : this(discriminator) { Boolean = value; }
        /// <summary>Create a new discriminated union value</summary>
        public DiscriminatedUnion32Object(int discriminator, object value) : this(value != null ? discriminator : 0) { Object = value; }

        /// <summary>Reset a value if the specified discriminator is assigned</summary>
        public static void Reset(ref DiscriminatedUnion32Object value, int discriminator)
        {
            if (value.Discriminator == discriminator) value = default;
        }
        /// <summary>The discriminator value</summary>
        public int Discriminator => _discriminator;
    }
}
