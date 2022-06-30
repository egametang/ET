#pragma once
#include <stdint.h>

namespace il2cpp
{
namespace os
{
    class CpuInfo
    {
    public:
        static void* Create();

        /*
        * This function returns the cpu usage in percentage,
        * normalized on the number of cores.
        *
        * Warning : the percentage returned can be > 100%. This
        * might happens on systems like Android which, for
        * battery and performance reasons, shut down cores and
        * lie about the number of active cores.
        */
        static int32_t Usage(void* previous);
    };
}
}
