#include "il2cpp-config.h"

#include "gc/GarbageCollector.h"

#if IL2CPP_ENABLE_WRITE_BARRIERS
void Il2CppCodeGenWriteBarrier(void** targetAddress, void* object)
{
    il2cpp::gc::GarbageCollector::SetWriteBarrier(targetAddress);
}

#endif


#if !RUNTIME_TINY

#include "utils/Runtime.h"
#include "vm/Class.h"
#include "vm/Field.h"
#include "vm/Type.h"

// This function exists to help with generation of callstacks for exceptions
// on iOS and MacOS x64 with clang 6.0 (newer versions of clang don't have this
// problem on x64). There we call the backtrace function, which does not play nicely
// with NORETURN, since the compiler eliminates the method prologue code setting up
// the address of the return frame (which makes sense). So on iOS we need to make
// the NORETURN define do nothing, then we use this dummy method which has the
// attribute for clang on iOS defined to prevent clang compiler errors for
// method that end by throwing a managed exception.
REAL_NORETURN IL2CPP_NO_INLINE void il2cpp_codegen_no_return()
{
    IL2CPP_UNREACHABLE;
}

REAL_NORETURN void il2cpp_codegen_abort()
{
    il2cpp::utils::Runtime::Abort();
    il2cpp_codegen_no_return();
}

#if IL2CPP_ENABLE_WRITE_BARRIERS
void Il2CppCodeGenWriteBarrierForType(const Il2CppType* type, void** targetAddress, void* object)
{
#if IL2CPP_ENABLE_STRICT_WRITE_BARRIERS
    if (il2cpp::vm::Type::IsPointerType(type))
        return;

    if (il2cpp::vm::Type::IsStruct(type))
    {
        Il2CppClass* klass = il2cpp::vm::Class::FromIl2CppType(type);

        FieldInfo* field;
        void* iter = NULL;
        while ((field = il2cpp::vm::Class::GetFields(klass, &iter)))
        {
            if (il2cpp::vm::Field::GetFlags(field) & FIELD_ATTRIBUTE_STATIC)
                continue;

            void* fieldTargetAddress = il2cpp::vm::Field::GetInstanceFieldDataPointer((void*)targetAddress, field);
            Il2CppCodeGenWriteBarrierForType(field->type, (void**)fieldTargetAddress, NULL);
        }
    }
    else
    {
        il2cpp::gc::GarbageCollector::SetWriteBarrier(targetAddress);
    }
#else
    il2cpp::gc::GarbageCollector::SetWriteBarrier(targetAddress);
#endif
}

void Il2CppCodeGenWriteBarrierForClass(Il2CppClass* klass, void** targetAddress, void* object)
{
#if IL2CPP_ENABLE_STRICT_WRITE_BARRIERS
    Il2CppCodeGenWriteBarrierForType(il2cpp::vm::Class::GetType(klass), targetAddress, object);
#else
    il2cpp::gc::GarbageCollector::SetWriteBarrier(targetAddress);
#endif
}

#endif // IL2CPP_ENABLE_WRITE_BARRIERS

#endif // !RUNTIME_TINY

#if IL2CPP_TINY

#include <cstdio>

int il2cpp_codegen_double_to_string(double value, uint8_t* format, uint8_t* buffer, int bufferLength)
{
    // return number of characters written to the buffer. if the return value greater than bufferLength
    // means the number of characters would be written to the buffer if there is enough space
    return snprintf(reinterpret_cast<char*>(buffer), bufferLength, reinterpret_cast<char*>(format), value);
}

#endif
