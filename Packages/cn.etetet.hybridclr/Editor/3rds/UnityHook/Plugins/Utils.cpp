//
//  Utils.cpp
//  MonoHookUtils_OSX
//
//  Created by Misaka-Mikoto on 2022/8/31.
//
#include <stdio.h>
#include <stdint.h>
#include <pthread.h>
#include <string.h>
#include <libkern/OSCacheControl.h>

extern "C"{

void* memcpy_jit(void* dst, void* src, int32_t size)
{
    pthread_jit_write_protect_np(0);
    void* ret = memcpy(dst, src, size);
    pthread_jit_write_protect_np(1);
    sys_icache_invalidate (dst, size);
    return ret;
}
}
