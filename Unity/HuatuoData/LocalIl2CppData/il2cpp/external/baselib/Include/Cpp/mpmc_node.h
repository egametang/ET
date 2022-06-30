#pragma once

#include "Atomic.h"

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        // mpmc_node container node class. All nodes used by mpmc_node containers must derive from this class.
        // No initialization or other restrictions apply. Inherited class is not accessed by the mpmc_node containers.
        class mpmc_node
        {
        public:
            baselib::atomic<mpmc_node*> next;
        };
    }
}
