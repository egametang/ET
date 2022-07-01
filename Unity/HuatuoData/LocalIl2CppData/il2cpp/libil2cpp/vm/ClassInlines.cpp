#include "ClassInlines.h"
#include "vm/Class.h"
#include "vm/Exception.h"
#include "vm/Method.h"
#include "vm/RCW.h"
#include "gc/GCHandle.h"

namespace il2cpp
{
namespace vm
{
    bool ClassInlines::InitFromCodegenSlow(Il2CppClass *klass)
    {
        bool result = Class::Init(klass);

        if (klass->has_initialization_error)
            il2cpp::vm::Exception::Raise((Il2CppException*)gc::GCHandle::GetTarget(klass->initializationExceptionGCHandle));

        return result;
    }

    NORETURN static void RaiseExceptionForNotFoundInterface(const Il2CppClass* klass, const Il2CppClass* itf, Il2CppMethodSlot slot)
    {
        std::string message;
        message = "Attempt to access method '" + Type::GetName(&itf->byval_arg, IL2CPP_TYPE_NAME_FORMAT_IL) + "." + Method::GetName(itf->methods[slot])
            + "' on type '" + Type::GetName(&klass->byval_arg, IL2CPP_TYPE_NAME_FORMAT_IL) + "' failed.";
        Exception::Raise(il2cpp::vm::Exception::GetMethodAccessException(message.c_str()));
    }

    const VirtualInvokeData* ClassInlines::GetInterfaceInvokeDataFromVTableSlowPath(const Il2CppClass* klass, const Il2CppClass* itf, Il2CppMethodSlot slot)
    {
        if (itf->generic_class != NULL)
        {
            Il2CppMetadataGenericContainerHandle containerHandle = MetadataCache::GetGenericContainerFromGenericClass(itf->image, itf->generic_class);

            for (uint16_t i = 0; i < klass->interface_offsets_count; ++i)
            {
                const Il2CppRuntimeInterfaceOffsetPair* pair = klass->interfaceOffsets + i;
                if (Class::IsGenericClassAssignableFrom(itf, pair->interfaceType, itf->image, containerHandle))
                {
                    IL2CPP_ASSERT(pair->offset + slot < klass->vtable_count);
                    return &klass->vtable[pair->offset + slot];
                }
            }
        }

        return NULL;
    }

    const VirtualInvokeData& ClassInlines::GetInterfaceInvokeDataFromVTableSlowPath(Il2CppObject* obj, const Il2CppClass* itf, Il2CppMethodSlot slot)
    {
        const Il2CppClass* klass = obj->klass;
        const VirtualInvokeData* data;

        data = GetInterfaceInvokeDataFromVTableSlowPath(klass, itf, slot);
        if (data)
            return *data;

        if (klass->is_import_or_windows_runtime)
        {
            Il2CppComObject* rcw = static_cast<Il2CppComObject*>(obj);

            // It might be null if it's called on a dead (already released) or fake object
            if (rcw->identity != NULL)
            {
                const VirtualInvokeData* invokeData = RCW::GetComInterfaceInvokeData(rcw, itf, slot);
                if (invokeData != NULL)
                {
                    // Nothing will be referencing these types directly, so we need to initialize them here
                    Class::Init(invokeData->method->klass);
                    return *invokeData;
                }
            }
        }

        RaiseExceptionForNotFoundInterface(klass, itf, slot);
        IL2CPP_UNREACHABLE;
    }
}
}
