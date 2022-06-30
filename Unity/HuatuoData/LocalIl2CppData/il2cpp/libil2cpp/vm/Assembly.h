#pragma once

#include <stdint.h>
#include <vector>
#include "il2cpp-config.h"
struct Il2CppAssembly;
struct Il2CppAssemblyName;
struct Il2CppImage;
struct Il2CppArray;

namespace il2cpp
{
namespace vm
{
    typedef std::vector<const Il2CppAssembly*> AssemblyVector;
    typedef std::vector<const Il2CppAssemblyName*> AssemblyNameVector;

    class LIBIL2CPP_CODEGEN_API Assembly
    {
// exported
    public:
        static Il2CppImage* GetImage(const Il2CppAssembly* assembly);
        static void GetReferencedAssemblies(const Il2CppAssembly* assembly, AssemblyNameVector* target);
    public:
        static AssemblyVector* GetAllAssemblies();
        static const Il2CppAssembly* GetLoadedAssembly(const char* name);
        static const Il2CppAssembly* Load(const char* name);
        static void Register(const Il2CppAssembly* assembly);
        static void ClearAllAssemblies();
        static void Initialize();

    private:
    };
} /* namespace vm */
} /* namespace il2cpp */
