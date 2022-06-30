#pragma once

#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"

struct Il2CppString;
struct Il2CppAssemblyName;
struct Il2CppReflectionAssembly;
struct mscorlib_System_Reflection_Assembly;
struct mscorlib_System_Reflection_Module;
struct mscorlib_System_Security_Policy_Evidence;
struct mscorlib_System_Reflection_AssemblyName;
struct Il2CppMonoAssemblyName;

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
    class LIBIL2CPP_CODEGEN_API Assembly
    {
    public:
        static intptr_t InternalGetReferencedAssemblies(Il2CppReflectionAssembly* module);
        static Il2CppReflectionAssembly* GetCallingAssembly();
        static Il2CppObject* GetEntryAssembly();
        static Il2CppReflectionAssembly* GetExecutingAssembly();
        static Il2CppReflectionAssembly* load_with_partial_name(Il2CppString* name, Il2CppObject* e);
        static Il2CppReflectionAssembly* LoadFile_internal(Il2CppString* assemblyFile, int32_t* stackMark);
        static Il2CppReflectionAssembly* LoadFrom(Il2CppString* assemblyFile, bool refOnly, int32_t* stackMark);
        static Il2CppReflectionType* InternalGetType(Il2CppReflectionAssembly* thisPtr, Il2CppObject* module, Il2CppString* name, bool throwOnError, bool ignoreCase);
        static Il2CppArray* GetTypes(Il2CppReflectionAssembly* thisPtr, bool exportedOnly);
        static void InternalGetAssemblyName(Il2CppString* assemblyFile, void* aname, Il2CppString** codebase);
    };
} /* namespace Reflection */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
