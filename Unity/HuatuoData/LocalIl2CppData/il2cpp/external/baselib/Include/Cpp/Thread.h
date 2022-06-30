#pragma once

#include "../C/Baselib_Thread.h"
#include "Time.h"

#include <memory>
#if !COMPILER_SUPPORTS_GENERIC_LAMBDA_EXPRESSIONS
#include <functional>
#endif

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        /*
        This class is not supposed to be used as-is.
        Instead separate thread class should be created to explicitely define thread lifetime.
        This is useful to avoid having timeout constants all over the codebase.

        class ApplicationThread : public baselib::Thread
        {
        public:
            // Expose base class constructors.
            using baselib::Thread::Thread;

            void Join()
            {
                // Thread must join with-in 10 seconds, or this is an error.
                // Use application specific methods to report error and/or try again.
                assert(baselib::Thread::TryJoin(10 * 1000) == true);
            }
        };

        */
        class BASELIB_API Thread
        {
        public:
            // Default constructor does nothing, useful when declaring thread as field in classes/structs
            Thread() = default;

            // Generic Constructor
            template<class FunctionType , class ... Args>
            Thread(FunctionType && f, Args && ... args)
            {
            #if COMPILER_SUPPORTS_GENERIC_LAMBDA_EXPRESSIONS
                // This generates cleaner and nicer-to-debug code
                auto wrapped = [ = ] {f(args ...);};
            #else
                auto wrapped = std::bind(f, args ...);
            #endif
                using Container = decltype(wrapped);

                // Small object optimization.
                constexpr bool smallObject = (sizeof(Container) <= sizeof(void*)) && (alignof(Container) <= alignof(void*));
                if (smallObject)
                {
                    union
                    {
                        // sizeof(void*) will trigger placement new errors
                        // even if code path is not executed
                        char buf[sizeof(Container)];
                        void* smallObject;
                    };
                    smallObject = nullptr; // to avoid -Wmaybe-uninitialized
                    // We have to move it to pointer, otherwise wrapped destructor will be called
                    new(buf) Container(std::move(wrapped));

                    thread = CreateThread(ThreadProxySmallObject<Container>, smallObject);
                }
                else
                {
                    std::unique_ptr<Container> ptr(new Container(std::move(wrapped)));
                    thread = CreateThread(ThreadProxyHeap<Container>, ptr.get());
                    if (thread)
                        ptr.release();
                }
            }

            // Thread has to be joined before destructor is called
            ~Thread();

            // Non-copyable
            Thread(const Thread&) = delete;
            Thread& operator=(const Thread&) = delete;

            // Movable
            Thread(Thread&& other);
            Thread& operator=(Thread&& other);

            // Return true if threads are supported
            static bool SupportsThreads();

            // Return true if join succeeded
            COMPILER_WARN_UNUSED_RESULT bool TryJoin(timeout_ms timeout);

            // Yields execution
            static inline void YieldExecution()
            {
                Baselib_Thread_YieldExecution();
            }

            // Returns thread id
            inline Baselib_Thread_Id GetId()
            {
                return Baselib_Thread_GetId(thread);
            }

            // Returns current thread id
            static inline Baselib_Thread_Id GetCurrentId()
            {
                return Baselib_Thread_GetCurrentThreadId();
            }

        private:
            Baselib_Thread* thread = nullptr;

            static Baselib_Thread* CreateThread(Baselib_Thread_EntryPointFunction function, void* arg);

            template<class T>
            static void ThreadProxyHeap(void* data)
            {
                std::unique_ptr<T> ptr(reinterpret_cast<T*>(data));
                (*ptr)();
            }

            template<class T>
            static void ThreadProxySmallObject(void* data)
            {
                T* ptr = reinterpret_cast<T*>(&data);
                (*ptr)();
                ptr->~T();
            }
        };
    }
}
