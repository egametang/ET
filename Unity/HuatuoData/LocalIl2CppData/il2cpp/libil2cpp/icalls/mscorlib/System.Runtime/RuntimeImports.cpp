#include "il2cpp-config.h"
#include "RuntimeImports.h"

#include <stdlib.h>
#include <algorithm>

#include "gc/GarbageCollector.h"
#include "vm/Class.h"
#include "vm/Type.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Runtime
{
#if IL2CPP_TARGET_WINDOWS
    void RuntimeImports::ecvt_s(char* buffer, int32_t sizeInBytes, double value, int32_t count, int32_t* dec, int32_t* sign)
    {
        errno_t returnValue = _ecvt_s(buffer, sizeInBytes, value, count, dec, sign);
        IL2CPP_ASSERT(returnValue == 0);
        NO_UNUSED_WARNING(returnValue);
    }

#endif

    void RuntimeImports::Memmove(uint8_t* dest, uint8_t* src, uint32_t len)
    {
        memmove(dest, src, len);
    }

    void RuntimeImports::Memmove_wbarrier(uint8_t* dest, uint8_t* src, uint32_t len, intptr_t type_handle)
    {
        uint32_t size = len * sizeof(void*);
        if (!vm::Type::IsReference((Il2CppType*)type_handle))
            size = vm::Class::GetValueSize(vm::Class::FromIl2CppType((Il2CppType*)type_handle), NULL) * len;

        memmove(dest, src, size);
        il2cpp::gc::GarbageCollector::SetWriteBarrier((void**)dest, size);
    }

    void RuntimeImports::ZeroMemory(void* p, uint32_t byteLength)
    {
        memset(p, 0, byteLength);
    }
} // namespace Runtime
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
