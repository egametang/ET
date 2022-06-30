#pragma once

#include "il2cpp-config.h"
#include "il2cpp-codegen-common.h"

#include <cmath>
#include <cstdlib>

#include "il2cpp-object-internals.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-tabledefs.h"

#include "vm-utils/Debugger.h"
#include "vm-utils/Finally.h"
#include "utils/ExceptionSupportStack.h"
#include "utils/Output.h"

REAL_NORETURN IL2CPP_NO_INLINE void il2cpp_codegen_no_return();

#if IL2CPP_COMPILER_MSVC
#define DEFAULT_CALL STDCALL
#else
#define DEFAULT_CALL
#endif

#if defined(__ARMCC_VERSION)
inline double bankers_round(double x)
{
    return __builtin_round(x);
}

inline float bankers_roundf(float x)
{
    return __builtin_roundf(x);
}

#else
inline double bankers_round(double x)
{
    double integerPart;
    if (x >= 0.0)
    {
        if (modf(x, &integerPart) == 0.5)
            return (int64_t)integerPart % 2 == 0 ? integerPart : integerPart + 1.0;
        return floor(x + 0.5);
    }
    else
    {
        if (modf(x, &integerPart) == -0.5)
            return (int64_t)integerPart % 2 == 0 ? integerPart : integerPart - 1.0;
        return ceil(x - 0.5);
    }
}

inline float bankers_roundf(float x)
{
    double integerPart;
    if (x >= 0.0f)
    {
        if (modf(x, &integerPart) == 0.5)
            return (int64_t)integerPart % 2 == 0 ? (float)integerPart : (float)integerPart + 1.0f;
        return floorf(x + 0.5f);
    }
    else
    {
        if (modf(x, &integerPart) == -0.5)
            return (int64_t)integerPart % 2 == 0 ? (float)integerPart : (float)integerPart - 1.0f;
        return ceilf(x - 0.5f);
    }
}

#endif

// returns true if overflow occurs
inline bool il2cpp_codegen_check_mul_overflow_i64(int64_t a, int64_t b, int64_t imin, int64_t imax)
{
    // TODO: use a better algorithm without division
    uint64_t ua = (uint64_t)llabs(a);
    uint64_t ub = (uint64_t)llabs(b);

    uint64_t c;
    if ((a > 0 && b > 0) || (a <= 0 && b <= 0))
        c = (uint64_t)llabs(imax);
    else
        c = (uint64_t)llabs(imin);

    return ua != 0 && ub > c / ua;
}

inline bool il2cpp_codegen_check_mul_oveflow_u64(uint64_t a, uint64_t b)
{
    return b != 0 && (a * b) / b != a;
}

inline int32_t il2cpp_codegen_abs(uint32_t value)
{
    return abs(static_cast<int32_t>(value));
}

inline int32_t il2cpp_codegen_abs(int32_t value)
{
    return abs(value);
}

inline int64_t il2cpp_codegen_abs(uint64_t value)
{
    return llabs(static_cast<int64_t>(value));
}

inline int64_t il2cpp_codegen_abs(int64_t value)
{
    return llabs(value);
}

// Exception support macros
#define IL2CPP_PUSH_ACTIVE_EXCEPTION(Exception) \
    __active_exceptions.push(Exception)

#define IL2CPP_POP_ACTIVE_EXCEPTION() \
    __active_exceptions.pop()

#define IL2CPP_GET_ACTIVE_EXCEPTION(ExcType) \
    (ExcType)__active_exceptions.top()

#define IL2CPP_RAISE_NULL_REFERENCE_EXCEPTION() \
    do {\
        il2cpp_codegen_raise_null_reference_exception();\
        il2cpp_codegen_no_return();\
    } while (0)

#define IL2CPP_RAISE_MANAGED_EXCEPTION(ex, lastManagedFrame) \
    do {\
        il2cpp_codegen_raise_exception((Exception_t*)ex, (RuntimeMethod*)lastManagedFrame);\
        il2cpp_codegen_no_return();\
    } while (0)

#define IL2CPP_RETHROW_MANAGED_EXCEPTION(ex) \
    do {\
        il2cpp_codegen_rethrow_exception((Exception_t*)ex);\
        il2cpp_codegen_no_return();\
    } while (0)

void il2cpp_codegen_memory_barrier();

template<typename T>
inline T VolatileRead(T* location)
{
    T result = *location;
    il2cpp_codegen_memory_barrier();
    return result;
}

template<typename T, typename U>
inline void VolatileWrite(T** location, U* value)
{
    il2cpp_codegen_memory_barrier();
    *location = value;
    Il2CppCodeGenWriteBarrier((void**)location, value);
}

template<typename T, typename U>
inline void VolatileWrite(T* location, U value)
{
    il2cpp_codegen_memory_barrier();
    *location = value;
}

inline void il2cpp_codegen_write_to_stdout(const char* str)
{
    il2cpp::utils::Output::WriteToStdout(str);
}

#if IL2CPP_TARGET_LUMIN
#include <stdarg.h>
#include <stdio.h>
inline void il2cpp_codegen_write_to_stdout_args(const char* str, ...)
{
    va_list args, local;
    char* buffer = nullptr;
    va_start(args, str);
    va_copy(local, args);
    int size = vsnprintf(nullptr, 0, str, local);
    if (size < 0)
    {
        va_end(local);
        va_end(args);
        return;
    }
    va_end(local);
    va_copy(local, args);
    buffer = new char[size + 1];
    vsnprintf(buffer, size + 1, str, local);
    il2cpp::utils::Output::WriteToStdout(buffer);
    if (buffer != nullptr)
        delete[] buffer;
    va_end(local);
    va_end(args);
}

#endif

inline void il2cpp_codegen_write_to_stderr(const char* str)
{
    il2cpp::utils::Output::WriteToStderr(str);
}

#if IL2CPP_TARGET_LUMIN
inline void il2cpp_codegen_write_to_stderr_args(const char* str, ...)
{
    va_list args, local;
    char* buffer = nullptr;
    va_start(args, str);
    va_copy(local, args);
    int size = vsnprintf(nullptr, 0, str, local);
    if (size < 0)
    {
        va_end(local);
        va_end(args);
        return;
    }
    va_end(local);
    va_copy(local, args);
    buffer = new char[size + 1];
    vsnprintf(buffer, size + 1, str, local);
    il2cpp::utils::Output::WriteToStderr(buffer);
    if (buffer != nullptr)
        delete[] buffer;
    va_end(local);
    va_end(args);
}

#endif

REAL_NORETURN void il2cpp_codegen_abort();

inline bool il2cpp_codegen_check_add_overflow(int64_t left, int64_t right)
{
    return (right >= 0 && left > kIl2CppInt64Max - right) ||
        (left < 0 && right < kIl2CppInt64Min - left);
}

inline bool il2cpp_codegen_check_sub_overflow(int64_t left, int64_t right)
{
    return (right >= 0 && left < kIl2CppInt64Min + right) ||
        (right < 0 && left > kIl2CppInt64Max + right);
}

inline void il2cpp_codegen_register_debugger_data(const Il2CppDebuggerMetadataRegistration *data)
{
#if IL2CPP_MONO_DEBUGGER
    il2cpp::utils::Debugger::RegisterMetadata(data);
#endif
}

inline void il2cpp_codegen_check_sequence_point(Il2CppSequencePointExecutionContext* executionContext, Il2CppSequencePoint* seqPoint)
{
#if IL2CPP_MONO_DEBUGGER
    il2cpp::utils::Debugger::CheckSequencePoint(executionContext, seqPoint);
#endif
}

inline void il2cpp_codegen_check_sequence_point_entry(Il2CppSequencePointExecutionContext* executionContext, Il2CppSequencePoint* seqPoint)
{
#if IL2CPP_MONO_DEBUGGER
    il2cpp::utils::Debugger::CheckSequencePointEntry(executionContext, seqPoint);
#endif
}

inline void il2cpp_codegen_check_sequence_point_exit(Il2CppSequencePointExecutionContext* executionContext, Il2CppSequencePoint* seqPoint)
{
#if IL2CPP_MONO_DEBUGGER
    il2cpp::utils::Debugger::CheckSequencePointExit(executionContext, seqPoint);
#endif
}

inline void il2cpp_codegen_check_pause_point()
{
#if IL2CPP_MONO_DEBUGGER
    il2cpp::utils::Debugger::CheckPausePoint();
#endif
}

class MethodExitSequencePointChecker
{
private:
    Il2CppSequencePoint* m_seqPoint;
    Il2CppSequencePointExecutionContext* m_seqPointStorage;

public:
    MethodExitSequencePointChecker(Il2CppSequencePointExecutionContext* seqPointStorage, Il2CppSequencePoint* seqPoint) :
        m_seqPointStorage(seqPointStorage), m_seqPoint(seqPoint)
    {
    }

    ~MethodExitSequencePointChecker()
    {
#if IL2CPP_MONO_DEBUGGER
        il2cpp_codegen_check_sequence_point_exit(m_seqPointStorage, m_seqPoint);
#endif
    }
};

#ifdef _MSC_VER
#define IL2CPP_DISABLE_OPTIMIZATIONS __pragma(optimize("", off))
#define IL2CPP_ENABLE_OPTIMIZATIONS __pragma(optimize("", on))
#elif IL2CPP_TARGET_LINUX
#define IL2CPP_DISABLE_OPTIMIZATIONS
#define IL2CPP_ENABLE_OPTIMIZATIONS
#else
#define IL2CPP_DISABLE_OPTIMIZATIONS __attribute__ ((optnone))
#define IL2CPP_ENABLE_OPTIMIZATIONS
#endif

// Array Unsafe
#define IL2CPP_ARRAY_UNSAFE_LOAD(TArray, TIndex) \
    (TArray)->GetAtUnchecked(static_cast<il2cpp_array_size_t>(TIndex))

inline bool il2cpp_codegen_object_reference_equals(const RuntimeObject *obj1, const RuntimeObject *obj2)
{
    return obj1 == obj2;
}

inline bool il2cpp_codegen_platform_is_osx_or_ios()
{
    return IL2CPP_TARGET_OSX != 0 || IL2CPP_TARGET_IOS != 0;
}

inline bool il2cpp_codegen_platform_is_freebsd()
{
    // we don't currently support FreeBSD
    return false;
}

inline bool il2cpp_codegen_platform_is_uwp()
{
    return IL2CPP_TARGET_WINRT != 0;
}

inline bool il2cpp_codegen_platform_disable_libc_pinvoke()
{
    return IL2CPP_PLATFORM_DISABLE_LIBC_PINVOKE;
}

template<typename T>
inline T il2cpp_unsafe_read_unaligned(void* location)
{
    T result;
#if IL2CPP_TARGET_ARMV7 || IL2CPP_TARGET_JAVASCRIPT
    memcpy(&result, location, sizeof(T));
#else
    result = *((T*)location);
#endif
    return result;
}

template<typename T>
inline void il2cpp_unsafe_write_unaligned(void* location, T value)
{
#if IL2CPP_TARGET_ARMV7 || IL2CPP_TARGET_JAVASCRIPT
    memcpy(location, &value, sizeof(T));
#else
    *((T*)location) = value;
#endif
}

template<typename T>
inline T il2cpp_unsafe_read(void* location)
{
    return *((T*)location);
}

template<typename T>
inline void il2cpp_unsafe_write(void* location, T value)
{
    *((T*)location) = value;
}

template<typename T, typename TOffset>
inline T* il2cpp_unsafe_add(void* source, TOffset offset)
{
    return reinterpret_cast<T*>(source) + offset;
}

template<typename T, typename TOffset>
inline T* il2cpp_unsafe_add_byte_offset(void* source, TOffset offset)
{
    return reinterpret_cast<T*>(reinterpret_cast<uint8_t*>(source) + offset);
}

template<typename T, typename TOffset>
inline T* il2cpp_unsafe_subtract(void* source, TOffset offset)
{
    return reinterpret_cast<T*>(source) - offset;
}

template<typename T, typename TOffset>
inline T* il2cpp_unsafe_subtract_byte_offset(void* source, TOffset offset)
{
    return reinterpret_cast<T*>(reinterpret_cast<uint8_t*>(source) - offset);
}

template<typename T>
inline T il2cpp_unsafe_as(void* source)
{
    return reinterpret_cast<T>(source);
}

template<typename T>
inline T* il2cpp_unsafe_as_ref(void* source)
{
    return reinterpret_cast<T*>(source);
}

inline void* il2cpp_unsafe_as_pointer(void* source)
{
    return source;
}

template<typename T>
inline T* il2cpp_unsafe_null_ref()
{
    return reinterpret_cast<T*>(NULL);
}

inline bool il2cpp_unsafe_are_same(void* left, void* right)
{
    return left == right;
}

inline bool il2cpp_unsafe_is_addr_gt(void* left, void* right)
{
    return left > right;
}

inline bool il2cpp_unsafe_is_addr_lt(void* left, void* right)
{
    return left < right;
}

inline bool il2cpp_unsafe_is_null_ref(void* source)
{
    return source == NULL;
}

template<typename T>
inline int32_t il2cpp_unsafe_sizeof()
{
    return sizeof(T);
}

inline intptr_t il2cpp_unsafe_byte_offset(void* origin, void* target)
{
    return reinterpret_cast<uint8_t*>(target) - reinterpret_cast<uint8_t*>(origin);
}
