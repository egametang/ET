#pragma once

struct Il2CppObject;

namespace il2cpp
{
namespace gc
{
    class WriteBarrier
    {
    public:
        static void GenericStore(void* ptr, void* value);
    };
} /* gc */
} /* il2cpp */
