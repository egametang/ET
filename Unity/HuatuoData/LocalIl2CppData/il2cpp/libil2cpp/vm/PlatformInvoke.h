#pragma once

#include "il2cpp-config.h"
#include "il2cpp-blob.h"
#include "il2cpp-runtime-metadata.h"
#include "il2cpp-object-internals.h"
#include "vm/Array.h"
#include "vm/Class.h"
#include "vm/MarshalAlloc.h"
#include "vm/Object.h"
#include "vm/String.h"
#include "utils/StringView.h"

#include <vector>
#include <string>

struct Il2CppString;
struct Il2CppStringBuilder;

struct PInvokeArguments;

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API PlatformInvoke
    {
    public:
        static void SetFindPluginCallback(Il2CppSetFindPlugInCallback method);
        static Il2CppMethodPointer Resolve(const PInvokeArguments& pinvokeArgs);

        static void MarshalFree(void* ptr);

        static char* MarshalCSharpStringToCppString(Il2CppString* managedString);
        static void MarshalCSharpStringToCppStringFixed(Il2CppString* managedString, char* buffer, int numberOfCharacters);
        static Il2CppChar* MarshalCSharpStringToCppWString(Il2CppString* managedString);
        static void MarshalCSharpStringToCppWStringFixed(Il2CppString* managedString, Il2CppChar* buffer, int numberOfCharacters);
        static il2cpp_hresult_t MarshalCSharpStringToCppBStringNoThrow(Il2CppString* managedString, Il2CppChar** bstr);
        static Il2CppChar* MarshalCSharpStringToCppBString(Il2CppString* managedString);

        static Il2CppString* MarshalCppStringToCSharpStringResult(const char* value);
        static Il2CppString* MarshalCppWStringToCSharpStringResult(const Il2CppChar* value);
        static Il2CppString* MarshalCppBStringToCSharpStringResult(const Il2CppChar* value);

        static void MarshalFreeBString(Il2CppChar* value);

        static char* MarshalEmptyStringBuilder(Il2CppStringBuilder* stringBuilder);
        static Il2CppChar* MarshalEmptyWStringBuilder(Il2CppStringBuilder* stringBuilder);

        static char* MarshalStringBuilder(Il2CppStringBuilder* stringBuilder);
        static Il2CppChar* MarshalWStringBuilder(Il2CppStringBuilder* stringBuilder);

        static void MarshalStringBuilderResult(Il2CppStringBuilder* stringBuilder, char* buffer);
        static void MarshalWStringBuilderResult(Il2CppStringBuilder* stringBuilder, Il2CppChar* buffer);

        static intptr_t MarshalDelegate(Il2CppDelegate* d);
        static Il2CppDelegate* MarshalFunctionPointerToDelegate(void* functionPtr, Il2CppClass* delegateType);

        template<typename T>
        static T* MarshalAllocateStringBuffer(size_t numberOfCharacters)
        {
            return (T*)MarshalAlloc::Allocate(numberOfCharacters * sizeof(T));
        }

    private:
        static char* MarshalEmptyStringBuilder(Il2CppStringBuilder* stringBuilder, size_t& stringLength, std::vector<std::string>& utf8Chunks, std::vector<Il2CppStringBuilder*>& builders);
        static Il2CppChar* MarshalEmptyWStringBuilder(Il2CppStringBuilder* stringBuilder, size_t& stringLength);
    };
} /* namespace vm */
} /* namespace il2cpp */
