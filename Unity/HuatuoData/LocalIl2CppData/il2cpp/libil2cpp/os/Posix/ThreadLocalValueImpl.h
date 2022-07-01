#pragma once

#if IL2CPP_THREADS_PTHREAD && !RUNTIME_TINY

#include "os/ErrorCodes.h"
#include "utils/NonCopyable.h"

#include <pthread.h>

namespace il2cpp
{
namespace os
{
    class ThreadLocalValueImpl : public il2cpp::utils::NonCopyable
    {
    public:
        inline ThreadLocalValueImpl()
        {
            pthread_key_t key;
            int result = pthread_key_create(&key, NULL);
            IL2CPP_ASSERT(result == 0);
            NO_UNUSED_WARNING(result);

            m_Key = key;
        }

        inline ~ThreadLocalValueImpl()
        {
            int result = pthread_key_delete(m_Key);
            IL2CPP_ASSERT(result == 0);
            NO_UNUSED_WARNING(result);
        }

        inline ErrorCode SetValue(void* value)
        {
#if IL2CPP_TARGET_DARWIN
            apple_pthread_setspecific_direct(m_Key, value);
#else
            if (pthread_setspecific(m_Key, value))
                return kErrorCodeGenFailure;
#endif

            return kErrorCodeSuccess;
        }

        inline ErrorCode GetValue(void** value)
        {
#if IL2CPP_TARGET_DARWIN
            *value = apple_pthread_getspecific_direct(m_Key);
#else
            *value = pthread_getspecific(m_Key);
#endif

            return kErrorCodeSuccess;
        }

    private:
#if IL2CPP_TARGET_DARWIN
        static inline void * apple_pthread_getspecific_direct(unsigned long slot)
        {
            void *ret;
        #if defined(__i386__) || defined(__x86_64__)
            __asm__ ("mov %%gs:%1, %0" : "=r" (ret) : "m" (*(void**)(slot * sizeof(void *))));
        #elif (defined(__arm__) && (defined(_ARM_ARCH_6) || defined(_ARM_ARCH_5)))
            void **__pthread_tsd;
        #if defined(__arm__) && defined(_ARM_ARCH_6)
            uintptr_t __pthread_tpid;
            __asm__ ("mrc p15, 0, %0, c13, c0, 3" : "=r" (__pthread_tpid));
            __pthread_tsd = (void**)(__pthread_tpid & ~0x3ul);
        #elif defined(__arm__) && defined(_ARM_ARCH_5)
            register uintptr_t __pthread_tpid asm ("r9");
            __pthread_tsd = (void**)__pthread_tpid;
        #endif
            ret = __pthread_tsd[slot];
        #elif defined(__arm64__)
            ret = pthread_getspecific(slot);
        #else
        #error no _pthread_getspecific_direct implementation for this arch
        #endif
            return ret;
        }

        inline static void apple_pthread_setspecific_direct(unsigned long slot, void * val)
        {
        #if defined(__i386__)
        #if defined(__PIC__)
            __asm__ ("movl %1,%%gs:%0" : "=m" (*(void**)(slot * sizeof(void *))) : "rn" (val));
        #else
            __asm__ ("movl %1,%%gs:%0" : "=m" (*(void**)(slot * sizeof(void *))) : "ri" (val));
        #endif
        #elif defined(__x86_64__)
            /* PIC is free and cannot be disabled, even with: gcc -mdynamic-no-pic ... */
            __asm__ ("movq %1,%%gs:%0" : "=m" (*(void**)(slot * sizeof(void *))) : "rn" (val));
        #elif (defined(__arm__) && (defined(_ARM_ARCH_6) || defined(_ARM_ARCH_5)))
            void **__pthread_tsd;
        #if defined(__arm__) && defined(_ARM_ARCH_6)
            uintptr_t __pthread_tpid;
            __asm__ ("mrc p15, 0, %0, c13, c0, 3" : "=r" (__pthread_tpid));
            __pthread_tsd = (void**)(__pthread_tpid & ~0x3ul);
        #elif defined(__arm__) && defined(_ARM_ARCH_5)
            register uintptr_t __pthread_tpid asm ("r9");
            __pthread_tsd = (void**)__pthread_tpid;
        #endif
            __pthread_tsd[slot] = val;
        #elif defined(__arm64__)
            pthread_setspecific(slot, val);
        #else
        #error no _pthread_setspecific_direct implementation for this arch
        #endif
        }

#endif
        pthread_key_t m_Key;
    };
}
}

#endif
