#pragma once

#include "../CommonDef.h"

namespace huatuo
{
namespace metadata
{

    class BlobReader
    {
    public:
        BlobReader(const byte* buf, uint32_t length) : _buf(buf), _length(length), _readPos(0)
        {

        }

        const byte* GetData() const
        {
            return _buf;
        }

        uint32_t GetLength() const
        {
            return _length;
        }

        uint32_t GetReadPosition() const
        {
            return _readPos;
        }

        bool IsEmpty() const
        {
            return _readPos >= _length;
        }

        bool NonEmpty() const
        {
            return _readPos < _length;
        }

        int32_t ReadCompressedInt32()
        {
            uint32_t unsignedValue = ReadCompressedUint32();
            uint32_t value = unsignedValue >> 1;
            if (!(unsignedValue & 0x1))
            {
                return value;
            }
            if (value < 0x40)
            {
                return value - 0x40;
            }
            if (value < 0x2000)
            {
                return value - 0x2000;
            }
            if (value < 0x10000000)
            {
                return value - 0x10000000;
            }
            IL2CPP_ASSERT(value < 0x20000000);
            return value - 0x20000000;
        }

        static uint32_t ReadCompressedUint32(const byte* buf, uint32_t& lengthSize)
        {
            uint32_t firstByte = buf[0];
            if (firstByte < 128)
            {
                lengthSize = 1;
                return firstByte;
            }
            else if (firstByte < 192)
            {
                lengthSize = 2;
                return ((firstByte & 0x3f) << 8) | buf[1];
            }
            else if (firstByte < 224)
            {
                lengthSize = 4;
                return ((firstByte & 0x1f) << 24) | (((uint32_t)buf[1]) << 16) | ((uint32_t)buf[2] << 8) | (uint32_t)buf[3];
            }
            else
            {
                RaiseHuatuoExecutionEngineException("bad metadata data. ReadEncodeLength fail");
                return 0;
            }
        }

        uint32_t ReadCompressedUint32()
        {
            uint32_t lengthSize;
            uint32_t value = ReadCompressedUint32(_buf + _readPos, lengthSize);
            _readPos += lengthSize;
            return value;
        }

        uint8_t ReadByte()
        {
            IL2CPP_ASSERT(_readPos < _length);
            return _buf[_readPos++];
        }

        uint16_t ReadUshort()
        {
            IL2CPP_ASSERT(_readPos + 2 <= _length);
            uint16_t value = *(uint16_t*)(_buf + _readPos);
            _readPos += 2;
            return value;
        }

        template<typename T>
        T Read()
        {
            IL2CPP_ASSERT(_readPos + sizeof(T) <= _length);
            T value = *(T*)(_buf + _readPos);
            _readPos += sizeof(T);
            return value;
        }

        uint8_t PeekByte()
        {
            IL2CPP_ASSERT(_readPos < _length);
            return _buf[_readPos];
        }

        void SkipByte()
        {
            IL2CPP_ASSERT(_readPos < _length);
            ++_readPos;
        }

        const byte* GetAndSkipCurBytes(uint32_t len)
        {
            IL2CPP_ASSERT(_readPos + len <= _length);
            const byte* data = _buf + _readPos;
            _readPos += len;
            return data;
        }

    private:
        const byte* const _buf;
        const uint32_t _length;
        uint32_t _readPos;
    };

}
}