#pragma once

#include <stdint.h>
#include "il2cpp-config.h"
#include "utils/StringView.h"
#include "il2cpp-class-internals.h"

struct Il2CppException;
struct Il2CppImage;
struct Il2CppClass;

namespace il2cpp
{
namespace vm
{
    class TypeNameParseInfo;

    class LIBIL2CPP_CODEGEN_API Exception
    {
// exported
    public:
        static Il2CppException* Get(il2cpp_hresult_t hresult, bool defaultToCOMException);

        static void PrepareExceptionForThrow(Il2CppException* ex, MethodInfo* lastManagedFrame = NULL);
        static NORETURN void Raise(Il2CppException* ex, MethodInfo* lastManagedFrame = NULL);
        static NORETURN void RaiseOutOfMemoryException();
        static NORETURN void RaiseOutOfMemoryException(const utils::StringView<Il2CppChar>& msg);
        static NORETURN void RaiseNullReferenceException();
        static NORETURN void RaiseNullReferenceException(const utils::StringView<Il2CppChar>& msg);
        static NORETURN void RaiseDivideByZeroException();
        static NORETURN void RaiseOverflowException();
        static NORETURN void RaiseArgumentOutOfRangeException(const char* msg);
        static NORETURN void Raise(il2cpp_hresult_t hresult, bool defaultToCOMException);

        inline static void RaiseIfFailed(il2cpp_hresult_t hresult, bool defaultToCOMException)
        {
            if (IL2CPP_HR_FAILED(hresult))
                Raise(hresult, defaultToCOMException);
        }

        ////TODO: rename to NewFromClassNameAndMessage
        static Il2CppException* FromNameMsg(const Il2CppImage* image, const char* name_space, const char* name, const char* msg);
        static Il2CppException* FromNameMsg(const Il2CppImage* image, const char* name_space, const char* name, const utils::StringView<Il2CppChar>& msg);
        static Il2CppException* FromNameMsg(const Il2CppImage* image, const char* name_space, const char* name, const utils::StringView<Il2CppChar>& msg, il2cpp_hresult_t hresult);

    public:
        ////TODO: rename all of these to NewXXX
        static Il2CppException* GetArgumentException(const char *arg, const char *msg);
        static Il2CppException* GetArgumentException(const utils::StringView<Il2CppChar>& arg, const utils::StringView<Il2CppChar>& msg);
        static Il2CppException* GetArgumentNullException(const char *arg);
        static Il2CppException* GetArgumentOutOfRangeException(const char *arg);
        static Il2CppException* GetTypeInitializationException(const char *msg, Il2CppException* innerException);
        static Il2CppException* GetIndexOutOfRangeException();
        static Il2CppException* GetIndexOutOfRangeException(const utils::StringView<Il2CppChar>& msg);
        static Il2CppException* GetNullReferenceException(const utils::StringView<Il2CppChar>& msg);
        static Il2CppException* GetInvalidCastException(const char* msg);
        static Il2CppException* GetInvalidCastException(const utils::StringView<Il2CppChar>& msg);
        static Il2CppException* GetTypeLoadException();
        static Il2CppException* GetTypeLoadException(const TypeNameParseInfo& typeNameParseInfo);
        static Il2CppException* GetTypeLoadException(const utils::StringView<char>& namespaze, const utils::StringView<char>& typeName, const utils::StringView<char>& assemblyName);
        static Il2CppException* GetTypeLoadExceptionForWindowsRuntimeType(const utils::StringView<char>& namespaze, const utils::StringView<char>& typeName);
        static Il2CppException* GetOutOfMemoryException(const utils::StringView<Il2CppChar>& msg);
        static Il2CppException* GetOverflowException();
        static Il2CppException* GetOverflowException(const char* msg);
        static Il2CppException* GetFormatException(const char* msg);
        static Il2CppException* GetSystemException();
        static Il2CppException* GetNotSupportedException(const char* msg);
        static Il2CppException* GetArrayTypeMismatchException();
        static Il2CppException* GetTypeLoadException(const char* msg);
        static Il2CppException* GetEntryPointNotFoundException(const char* msg);
        static Il2CppException* GetDllNotFoundException(const char* msg);
        static Il2CppException* GetInvalidOperationException(const char* msg);
        static Il2CppException* GetThreadInterruptedException();
        static Il2CppException* GetThreadAbortException();
        static Il2CppException* GetThreadStateException(const char* msg);
        static Il2CppException* GetSynchronizationLockException(const char* msg);
        static Il2CppException* GetMissingMethodException(const char* msg);
        static Il2CppException* GetMarshalDirectiveException(const char* msg);
        static Il2CppException* GetTargetException(const char* msg);
        static Il2CppException* GetExecutionEngineException(const char* msg);
        static Il2CppException* GetMethodAccessException(const char* msg);
        static Il2CppException* GetUnauthorizedAccessException(const utils::StringView<Il2CppChar>& msg);
        static Il2CppException* GetDivideByZeroException();
        static Il2CppException* GetPlatformNotSupportedException(const utils::StringView<Il2CppChar>& msg);
        static Il2CppException* GetFileLoadException(const char* msg);
        static Il2CppException* GetFileNotFoundException(const utils::StringView<Il2CppChar>& msg);

        static Il2CppException* GetMaxmimumNestedGenericsException();

        // ==={{ huatuo
        static Il2CppException* GetStackOverflowException(const char* msg);
        static Il2CppException* GetBadImageFormatException(const char* msg);
        static Il2CppException* GetMissingFieldException(const char* msg);
        // ===}} huatuo

        static void StoreExceptionInfo(Il2CppException* ex, Il2CppString* exceptionString);
    };
} /* namespace vm */
} /* namespace il2cpp */
