#pragma once

#include "il2cpp-object-internals.h"

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
    class LIBIL2CPP_CODEGEN_API RuntimeAssembly
    {
    public:
        static bool get_global_assembly_cache(Il2CppObject* thisPtr);
        static bool get_ReflectionOnly(Il2CppObject* thisPtr);
        static bool GetAotIdInternal(Il2CppArray* aotid);
        static bool GetManifestResourceInfoInternal(Il2CppReflectionAssembly* assembly, Il2CppString* name, Il2CppManifestResourceInfo* info);
        static intptr_t GetManifestResourceInternal(Il2CppReflectionAssembly* assembly, Il2CppString* name, int* size, Il2CppReflectionModule** module);
        static Il2CppObject* GetFilesInternal(Il2CppObject* thisPtr, Il2CppString* name, bool getResourceModules);
        static Il2CppReflectionMethod* get_EntryPoint(Il2CppReflectionAssembly* self);
        static Il2CppObject* GetManifestModuleInternal(Il2CppObject* thisPtr);
        static Il2CppArray* GetModulesInternal(Il2CppReflectionAssembly * thisPtr);
        static Il2CppString* get_code_base(Il2CppReflectionAssembly* reflectionAssembly, bool escaped);
        static Il2CppString* get_fullname(Il2CppReflectionAssembly* assembly);
        static Il2CppString* get_location(Il2CppObject* thisPtr);
        static Il2CppString* InternalImageRuntimeVersion(Il2CppObject* a);
        static Il2CppArray* GetManifestResourceNames(Il2CppReflectionAssembly* assembly);
    };
} // namespace Reflection
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
