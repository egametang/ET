#pragma once

#include "il2cpp-config.h"

#if IL2CPP_GC_BOEHM

/* here is the defines we build Boehm with */
    #define IGNORE_DYNAMIC_LOADING 1
    #define GC_DONT_REGISTER_MAIN_STATIC_DATA 1
    #if IL2CPP_HAS_GC_DESCRIPTORS
    #define GC_GCJ_SUPPORT 1
    #endif
    #if IL2CPP_SUPPORT_THREADS
        #define GC_THREADS 1
    #endif

    #include "gc.h"
    #include "gc_typed.h"
    #include "gc_mark.h"
    #include "gc_gcj.h"
    #include "gc_vector.h"

    #define GC_NO_DESCRIPTOR ((void*)(0 | GC_DS_LENGTH))

#else
    #define GC_NO_DESCRIPTOR ((void*)0)

#endif
