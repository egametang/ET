using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace MemoryPack {

// VarInt, first sbyte is value or typeCode

// 0~127 = unsigned byte value
// -1~-120 = signed byte value
// -121 = byte
// -122 = sbyte
// -123 = ushort
// -124 = short
// -125 = uint
// -126 = int
// -127 = ulong
// -128 = long 

internal static class VarIntCodes
{
    public const byte MaxSingleValue = 127;
    public const sbyte MinSingleValue = -120;

    public const sbyte Byte = -121;
    public const sbyte SByte = -122;
    public const sbyte UInt16 = -123;
    public const sbyte Int16 = -124;
    public const sbyte UInt32 = -125;
    public const sbyte Int32 = -126;
    public const sbyte UInt64 = -127;
    public const sbyte Int64 = -128;
}

public ref partial struct MemoryPackWriter
{
    public void WriteVarInt(byte x)
    {
        if (x <= VarIntCodes.MaxSingleValue)
        {
            WriteUnmanaged((sbyte)x);
        }
        else
        {
            WriteUnmanaged(VarIntCodes.Byte, x);
        }
    }

    public void WriteVarInt(sbyte x)
    {
        if (VarIntCodes.MinSingleValue <= x)
        {
            WriteUnmanaged(x);
        }
        else
        {
            WriteUnmanaged(VarIntCodes.SByte, x);
        }
    }

    public void WriteVarInt(ushort x)
    {
        if (x <= VarIntCodes.MaxSingleValue)
        {
            WriteUnmanaged((sbyte)x);
        }
        else
        {
            WriteUnmanaged(VarIntCodes.UInt16, (UInt16)x);
        }
    }

    public void WriteVarInt(short x)
    {
        if (0 <= x)
        {
            if (x <= VarIntCodes.MaxSingleValue) // same as sbyte.MaxValue
            {
                WriteUnmanaged((sbyte)x);
            }
            else
            {
                WriteUnmanaged(VarIntCodes.Int16, (Int16)x);
            }
        }
        else
        {
            if (VarIntCodes.MinSingleValue <= x)
            {
                WriteUnmanaged((sbyte)x);
            }
            else if (sbyte.MinValue <= x)
            {
                WriteUnmanaged(VarIntCodes.SByte, (SByte)x);
            }
            else
            {
                WriteUnmanaged(VarIntCodes.Int16, (Int16)x);
            }
        }
    }

    public void WriteVarInt(uint x)
    {
        if (x <= VarIntCodes.MaxSingleValue)
        {
            WriteUnmanaged((sbyte)x);
        }
        else if (x <= ushort.MaxValue)
        {
            WriteUnmanaged(VarIntCodes.UInt16, (UInt16)x);
        }
        else
        {
            WriteUnmanaged(VarIntCodes.UInt32, (UInt32)x);
        }
    }

    public void WriteVarInt(int x)
    {
        if (0 <= x)
        {
            if (x <= VarIntCodes.MaxSingleValue) // same as sbyte.MaxValue
            {
                WriteUnmanaged((sbyte)x);
            }
            else if (x <= short.MaxValue)
            {
                WriteUnmanaged(VarIntCodes.Int16, (Int16)x);
            }
            else
            {
                WriteUnmanaged(VarIntCodes.Int32, (Int32)x);
            }
        }
        else
        {
            if (VarIntCodes.MinSingleValue <= x)
            {
                WriteUnmanaged((sbyte)x);
            }
            else if (sbyte.MinValue <= x)
            {
                WriteUnmanaged(VarIntCodes.SByte, (SByte)x);
            }
            else if (short.MinValue <= x)
            {
                WriteUnmanaged(VarIntCodes.Int16, (Int16)x);
            }
            else
            {
                WriteUnmanaged(VarIntCodes.Int32, (Int32)x);
            }
        }
    }

    public void WriteVarInt(ulong x)
    {
        if (x <= VarIntCodes.MaxSingleValue)
        {
            WriteUnmanaged((sbyte)x);
        }
        else if (x <= ushort.MaxValue)
        {
            WriteUnmanaged(VarIntCodes.UInt16, (UInt16)x);
        }
        else if (x <= uint.MaxValue)
        {
            WriteUnmanaged(VarIntCodes.UInt32, (UInt32)x);
        }
        else
        {
            WriteUnmanaged(VarIntCodes.UInt64, (UInt64)x);
        }
    }

    public void WriteVarInt(long x)
    {
        if (0 <= x)
        {
            if (x <= VarIntCodes.MaxSingleValue) // same as sbyte.MaxValue
            {
                WriteUnmanaged((sbyte)x);
            }
            else if (x <= short.MaxValue)
            {
                WriteUnmanaged(VarIntCodes.Int16, (Int16)x);
            }
            else if (x <= int.MaxValue)
            {
                WriteUnmanaged(VarIntCodes.Int32, (Int32)x);
            }
            else
            {
                WriteUnmanaged(VarIntCodes.Int64, (Int64)x);
            }
        }
        else
        {
            if (VarIntCodes.MinSingleValue <= x)
            {
                WriteUnmanaged((sbyte)x);
            }
            else if (sbyte.MinValue <= x)
            {
                WriteUnmanaged(VarIntCodes.SByte, (SByte)x);
            }
            else if (short.MinValue <= x)
            {
                WriteUnmanaged(VarIntCodes.Int16, (Int16)x);
            }
            else if (int.MinValue <= x)
            {
                WriteUnmanaged(VarIntCodes.Int32, (Int32)x);
            }
            else
            {
                WriteUnmanaged(VarIntCodes.Int64, (Int64)x);
            }
        }
    }
}

public ref partial struct MemoryPackReader
{
    public byte ReadVarIntByte()
    {
        ReadUnmanaged(out sbyte typeCode);

        switch (typeCode)
        {
            case VarIntCodes.Byte:
                return ReadUnmanaged<byte>();
            case VarIntCodes.SByte:
                return checked((byte)ReadUnmanaged<sbyte>());
            case VarIntCodes.UInt16:
                return checked((byte)ReadUnmanaged<byte>());
            case VarIntCodes.Int16:
                return checked((byte)ReadUnmanaged<short>());
            case VarIntCodes.UInt32:
                return checked((byte)ReadUnmanaged<uint>());
            case VarIntCodes.Int32:
                return checked((byte)ReadUnmanaged<int>());
            case VarIntCodes.UInt64:
                return checked((byte)ReadUnmanaged<ulong>());
            case VarIntCodes.Int64:
                return checked((byte)ReadUnmanaged<long>());
            default:
                return checked((byte)typeCode);
        }
    }

    public sbyte ReadVarIntSByte()
    {
        ReadUnmanaged(out sbyte typeCode);

        switch (typeCode)
        {
            case VarIntCodes.Byte:
                return checked((sbyte)ReadUnmanaged<byte>());
            case VarIntCodes.SByte:
                return ReadUnmanaged<sbyte>();
            case VarIntCodes.UInt16:
                return checked((sbyte)ReadUnmanaged<ushort>());
            case VarIntCodes.Int16:
                return checked((sbyte)ReadUnmanaged<short>());
            case VarIntCodes.UInt32:
                return checked((sbyte)ReadUnmanaged<uint>());
            case VarIntCodes.Int32:
                return checked((sbyte)ReadUnmanaged<int>());
            case VarIntCodes.UInt64:
                return checked((sbyte)ReadUnmanaged<ulong>());
            case VarIntCodes.Int64:
                return checked((sbyte)ReadUnmanaged<long>());
            default:
                return typeCode;
        }
    }

    public ushort ReadVarIntUInt16()
    {
        ReadUnmanaged(out sbyte typeCode);

        switch (typeCode)
        {
            case VarIntCodes.Byte:
                return ReadUnmanaged<byte>();
            case VarIntCodes.SByte:
                return checked((ushort)ReadUnmanaged<sbyte>());
            case VarIntCodes.UInt16:
                return ReadUnmanaged<ushort>();
            case VarIntCodes.Int16:
                return checked((ushort)ReadUnmanaged<short>());
            case VarIntCodes.UInt32:
                return checked((ushort)ReadUnmanaged<uint>());
            case VarIntCodes.Int32:
                return checked((ushort)ReadUnmanaged<int>());
            case VarIntCodes.UInt64:
                return checked((ushort)ReadUnmanaged<ulong>());
            case VarIntCodes.Int64:
                return checked((ushort)ReadUnmanaged<long>());
            default:
                return checked((ushort)typeCode);
        }
    }

    public short ReadVarIntInt16()
    {
        ReadUnmanaged(out sbyte typeCode);

        switch (typeCode)
        {
            case VarIntCodes.Byte:
                return ReadUnmanaged<byte>();
            case VarIntCodes.SByte:
                return ReadUnmanaged<sbyte>();
            case VarIntCodes.UInt16:
                return checked((short)ReadUnmanaged<ushort>());
            case VarIntCodes.Int16:
                return ReadUnmanaged<short>();
            case VarIntCodes.UInt32:
                return checked((short)ReadUnmanaged<uint>());
            case VarIntCodes.Int32:
                return checked((short)ReadUnmanaged<int>());
            case VarIntCodes.UInt64:
                return checked((short)ReadUnmanaged<ulong>());
            case VarIntCodes.Int64:
                return checked((short)ReadUnmanaged<long>());
            default:
                return typeCode;
        }
    }

    public uint ReadVarIntUInt32()
    {
        ReadUnmanaged(out sbyte typeCode);

        switch (typeCode)
        {
            case VarIntCodes.Byte:
                return ReadUnmanaged<byte>();
            case VarIntCodes.SByte:
                return checked((uint)ReadUnmanaged<sbyte>());
            case VarIntCodes.UInt16:
                return ReadUnmanaged<ushort>();
            case VarIntCodes.Int16:
                return checked((uint)ReadUnmanaged<short>());
            case VarIntCodes.UInt32:
                return ReadUnmanaged<uint>();
            case VarIntCodes.Int32:
                return checked((uint)ReadUnmanaged<int>());
            case VarIntCodes.UInt64:
                return checked((uint)ReadUnmanaged<ulong>());
            case VarIntCodes.Int64:
                return checked((uint)ReadUnmanaged<long>());
            default:
                return checked((uint)typeCode);
        }
    }

    public int ReadVarIntInt32()
    {
        ReadUnmanaged(out sbyte typeCode);

        switch (typeCode)
        {
            case VarIntCodes.Byte:
                return ReadUnmanaged<byte>();
            case VarIntCodes.SByte:
                return ReadUnmanaged<sbyte>();
            case VarIntCodes.UInt16:
                return ReadUnmanaged<ushort>();
            case VarIntCodes.Int16:
                return ReadUnmanaged<short>();
            case VarIntCodes.UInt32:
                return checked((int)ReadUnmanaged<uint>());
            case VarIntCodes.Int32:
                return ReadUnmanaged<int>();
            case VarIntCodes.UInt64:
                return checked((int)ReadUnmanaged<ulong>());
            case VarIntCodes.Int64:
                return checked((int)ReadUnmanaged<long>());
            default:
                return typeCode;
        }
    }

    public ulong ReadVarIntUInt64()
    {
        ReadUnmanaged(out sbyte typeCode);

        switch (typeCode)
        {
            case VarIntCodes.Byte:
                return ReadUnmanaged<byte>();
            case VarIntCodes.SByte:
                return checked((ulong)ReadUnmanaged<sbyte>());
            case VarIntCodes.UInt16:
                return ReadUnmanaged<ushort>();
            case VarIntCodes.Int16:
                return checked((ulong)ReadUnmanaged<short>());
            case VarIntCodes.UInt32:
                return ReadUnmanaged<uint>();
            case VarIntCodes.Int32:
                return checked((ulong)ReadUnmanaged<int>());
            case VarIntCodes.UInt64:
                return ReadUnmanaged<ulong>();
            case VarIntCodes.Int64:
                return checked((ulong)ReadUnmanaged<long>());
            default:
                return checked((ulong)typeCode);
        }
    }

    public long ReadVarIntInt64()
    {
        ReadUnmanaged(out sbyte typeCode);

        switch (typeCode)
        {
            case VarIntCodes.Byte:
                return ReadUnmanaged<byte>();
            case VarIntCodes.SByte:
                return ReadUnmanaged<sbyte>();
            case VarIntCodes.UInt16:
                return ReadUnmanaged<ushort>();
            case VarIntCodes.Int16:
                return ReadUnmanaged<short>();
            case VarIntCodes.UInt32:
                return ReadUnmanaged<uint>();
            case VarIntCodes.Int32:
                return ReadUnmanaged<int>();
            case VarIntCodes.UInt64:
                return checked((long)ReadUnmanaged<ulong>());
            case VarIntCodes.Int64:
                return ReadUnmanaged<long>();
            default:
                return typeCode;
        }
    }
}

}