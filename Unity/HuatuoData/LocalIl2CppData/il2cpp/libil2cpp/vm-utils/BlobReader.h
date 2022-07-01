#pragma once

#include "../il2cpp-blob.h"

namespace il2cpp
{
namespace utils
{
    class BlobReader
    {
    public:
        // internal
        static int GetConstantValueFromBlob(Il2CppTypeEnum type, const char *blob, void *value);
    };
} /* utils */
} /* il2cpp */
