#pragma once

#include <stdint.h>
#include "il2cpp-config.h"
#include "utils/StringView.h"

struct Il2CppString;

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API String
    {
    public:
        //exported
        static Il2CppString* Empty();
        static int32_t GetLength(Il2CppString* str);
        static int32_t GetHash(Il2CppString* str);
        static Il2CppString* New(const char* str);
        static Il2CppString* NewWrapper(const char* str);
        static Il2CppString* NewLen(const char* str, uint32_t length);
        static Il2CppString* NewSize(int32_t len);
        static Il2CppString* NewUtf16(const Il2CppChar *text, int32_t len);
        static Il2CppString* NewUtf16(const utils::StringView<Il2CppChar>& text);

    public:
        static void InitializeEmptyString(Il2CppClass* stringClass);
        static void CleanupEmptyString();
        static Il2CppString* Intern(Il2CppString* str);
        static Il2CppString* IsInterned(Il2CppString* str);
    };
} /* namespace vm */
} /* namespace il2cpp */
