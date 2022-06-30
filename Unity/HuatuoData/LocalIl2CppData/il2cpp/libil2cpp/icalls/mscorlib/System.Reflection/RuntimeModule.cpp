#include "il2cpp-config.h"
#include "RuntimeModule.h"
#include "il2cpp-class-internals.h"
#include "vm/Exception.h"
#include "vm/Image.h"
#include "vm/Array.h"
#include "vm/Module.h"
#include "vm/Reflection.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Reflection
{
    Il2CppArray* RuntimeModule::ResolveSignature(intptr_t module, int32_t metadataToken, int32_t* error)
    {
        NOT_SUPPORTED_IL2CPP(Module::ResolveSignature, "This icall is not supported by il2cpp.");

        return 0;
    }

    int32_t RuntimeModule::get_MetadataToken(Il2CppReflectionModule* module)
    {
        return vm::Module::GetToken(module->image);
    }

    int32_t RuntimeModule::GetMDStreamVersion(intptr_t module)
    {
        NOT_SUPPORTED_IL2CPP(Module::GetMDStreamVersion, "This icall is not supported by il2cpp.");

        return 0;
    }

    intptr_t RuntimeModule::GetHINSTANCE(intptr_t module)
    {
        NOT_SUPPORTED_IL2CPP(RuntimeModule::GetHINSTANCE,  "This icall is not supported by il2cpp.");
        return 0;
    }

    intptr_t RuntimeModule::ResolveFieldToken(intptr_t module, int32_t token, Il2CppArray* type_args, Il2CppArray* method_args, int32_t* error)
    {
        NOT_SUPPORTED_IL2CPP(Module::ResolveFieldToken, "This icall is not supported by il2cpp.");

        return intptr_t();
    }

    intptr_t RuntimeModule::ResolveMethodToken(intptr_t module, int32_t token, Il2CppArray* type_args, Il2CppArray* method_args, int32_t* error)
    {
        NOT_SUPPORTED_IL2CPP(Module::ResolveMethodToken, "This icall is not supported by il2cpp.");

        return intptr_t();
    }

    intptr_t RuntimeModule::ResolveTypeToken(intptr_t module, int32_t token, Il2CppArray* type_args, Il2CppArray* method_args, int32_t* error)
    {
        NOT_SUPPORTED_IL2CPP(Module::ResolveTypeToken, "This icall is not supported by il2cpp.");

        return intptr_t();
    }

    Il2CppObject* RuntimeModule::ResolveMemberToken(intptr_t module, int32_t token, Il2CppArray* type_args, Il2CppArray* method_args, int32_t* error)
    {
        NOT_SUPPORTED_IL2CPP(Module::ResolveMemberToken, "This icall is not supported by il2cpp.");

        return 0;
    }

    Il2CppString* RuntimeModule::ResolveStringToken(intptr_t module, int32_t token, int32_t* error)
    {
        NOT_SUPPORTED_IL2CPP(Module::ResolveStringToken, "This icall is not supported by il2cpp.");

        return 0;
    }

    Il2CppObject* RuntimeModule::GetGlobalType(intptr_t module)
    {
        NOT_SUPPORTED_IL2CPP(Module::GetGlobalType, "This icall is not supported by il2cpp.");

        return 0;
    }

    Il2CppArray* RuntimeModule::InternalGetTypes(const Il2CppImage* image)
    {
        return il2cpp::vm::Image::GetTypes(image, false);
    }

    void RuntimeModule::GetGuidInternal(intptr_t module, Il2CppArray* guid)
    {
        // No implementation on purpose. The guid array will be unchanged, as IL2CPP
        // does not support GUIDs for modules. But we don't want to throw an
        // exception here.
    }

    void RuntimeModule::GetPEKind(intptr_t module, int32_t* peKind, int32_t* machine)
    {
        NOT_SUPPORTED_IL2CPP(Module::GetPEKind, "This icall is not supported by il2cpp.");
    }
} // namespace Reflection
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
