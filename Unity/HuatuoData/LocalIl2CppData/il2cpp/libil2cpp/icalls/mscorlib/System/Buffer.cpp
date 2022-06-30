#include "il2cpp-config.h"
#include <memory>
#include "icalls/mscorlib/System/Buffer.h"
#include "il2cpp-class-internals.h"
#include "vm/Array.h"
#include "vm/Class.h"
#include "vm/Exception.h"

using il2cpp::vm::Class;


namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    bool Buffer::InternalBlockCopy(Il2CppArray* src, int32_t srcOffsetBytes, Il2CppArray* dest, int32_t dstOffsetBytes, int32_t count)
    {
        IL2CPP_CHECK_ARG_NULL(src);
        IL2CPP_CHECK_ARG_NULL(dest);

        /* watch out for integer overflow */
        if (((uint32_t)srcOffsetBytes > il2cpp::vm::Array::GetByteLength(src) - count) || ((uint32_t)dstOffsetBytes > il2cpp::vm::Array::GetByteLength(dest) - count))
            return false;

        char* src_buf = ((char*)il2cpp_array_addr_with_size(src, Class::GetInstanceSize(src->klass->element_class), 0)) + srcOffsetBytes;
        char* dest_buf = ((char*)il2cpp_array_addr_with_size(dest, Class::GetInstanceSize(dest->klass->element_class), 0)) + dstOffsetBytes;

        if (src != dest)
            memcpy(dest_buf, src_buf, count);
        else
            memmove(dest_buf, src_buf, count); /* Source and dest are the same array */

        return true;
    }

    int32_t Buffer::_ByteLength(Il2CppArray* array)
    {
        return il2cpp::vm::Array::GetByteLength(array);
    }

    void Buffer::InternalMemcpy(uint8_t* dest, uint8_t* src, int32_t count)
    {
        memcpy(dest, src, count);
    }
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
