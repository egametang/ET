#pragma once

#include "il2cpp-class-internals.h"
#include "../il2cpp-blob.h"

namespace il2cpp
{
namespace utils
{
    class BlobReader
    {
    public:
        // internal
        static bool GetConstantValueFromBlob(const Il2CppImage* image, Il2CppTypeEnum type, const char *blob, void *value);
        // This overload move the blob pointer as it reads
        static bool GetConstantValueFromBlob(const Il2CppImage* image, Il2CppTypeEnum type, const char **blob, void *value, bool deserializeManagedObjects);

        // Reads an encoded Il2CppTypeEnum
        // For IL2CPP_TYPE_ENUM, the underlying Il2CppTypeEnum will be returned and klass will be the actual enum class
        // For IL2CPP_TYPE_SZARRAY, klass will just be System.Array
        // For all other Il2CppTypeEnum klass will the the correct Il2CppClass
        static Il2CppTypeEnum ReadEncodedTypeEnum(const Il2CppImage* image, const char** blob, Il2CppClass** klass);
    };
} /* utils */
} /* il2cpp */
