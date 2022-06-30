#pragma once

#include "../C/Baselib_ThreadLocalStorage.h"

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        // Thread Local Storage provides a variable that can be global but have different value in every thread.
        // For more details see Baselib_ThreadLocalStorage.
        // On some platforms this might be fiber local storage.
        //
        // Example of usage:
        // static ThreadLocalStorage<int32_t> threadErrorState;
        template<typename T>
        class ThreadLocalStorage
        {
        public:
            // by nature of TLS slots, they must be non-copyable, so
            ThreadLocalStorage(const ThreadLocalStorage & other) = delete;
            ThreadLocalStorage& operator=(const ThreadLocalStorage & other) = delete;

            ThreadLocalStorage()
            {
                static_assert(sizeof(T) <= sizeof(uintptr_t), "Provided type is too large to be stored in ThreadLocalStorage");
                handle = Baselib_TLS_Alloc();
            }

            ~ThreadLocalStorage()
            {
                if (IsValid())
                {
                    Baselib_TLS_Free(handle);
                    handle = InvalidTLSHandle;
                }
            }

            ThreadLocalStorage(ThreadLocalStorage && other)
            {
                // ensure that we don't leak local handle
                if (handle != InvalidTLSHandle)
                    Baselib_TLS_Free(handle);
                handle = other.handle;
                other.handle = InvalidTLSHandle;
            }

            // Check if variable is valid.
            // The only case when variable might be invalid is if it was moved to some other instance.
            inline bool IsValid() const
            {
                return handle != InvalidTLSHandle;
            }

            // Resets value in all threads.
            void Reset()
            {
                Baselib_TLS_Free(handle);
                handle = Baselib_TLS_Alloc();
            }

            inline T operator=(T value)
            {
                Baselib_TLS_Set(handle, (uintptr_t)value);
                return value;
            }

            inline ThreadLocalStorage<T>& operator=(ThreadLocalStorage&& other)
            {
                // swap values
                Baselib_TLS_Handle t = handle;
                handle = other.handle;
                other.handle = t;
                return *this;
            }

            inline operator T() const
            {
                return (T)Baselib_TLS_Get(handle);
            }

            inline T operator->() const
            {
                return (T)Baselib_TLS_Get(handle);
            }

            inline T operator++()
            {
                *this = *this + 1;
                return *this;
            }

            inline T operator--()
            {
                *this = *this - 1;
                return *this;
            }

        private:
            Baselib_TLS_Handle handle = InvalidTLSHandle;
            static constexpr uintptr_t InvalidTLSHandle = UINTPTR_MAX;
        };
    }
}
