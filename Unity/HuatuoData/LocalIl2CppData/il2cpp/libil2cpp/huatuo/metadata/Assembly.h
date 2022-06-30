#pragma once

#include "../CommonDef.h"

namespace huatuo
{
namespace metadata
{

    class Assembly
    {
    public:
        static Il2CppAssembly* LoadFromFile(const char* assemblyFile);
        static Il2CppAssembly* LoadFromBytes(const void* assemblyData, uint64_t length, bool copyData);
    private:
        static Il2CppAssembly* Create(const byte* assemblyData, uint64_t length, bool copyData);
    };
}
}