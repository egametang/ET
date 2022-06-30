#pragma once

#include "Atomic.h"

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        // mpsc_node container node class. All nodes used by mpsc_node containers must derive from this class.
        // No initialization or other restrictions apply. Inherited class is not accessed by the mpsc_node containers.
        class mpsc_node
        {
        public:
            atomic<mpsc_node*> next;
        };
    }
}
