#pragma once

#include <stdint.h>
#include "il2cpp-config.h"
#include "il2cpp-metadata.h"

struct Il2CppClass;
struct Il2CppGenericContainer;
struct Il2CppGenericParameter;

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API GenericContainer
    {
    public:
        // exported

    public:
        //internal
        static Il2CppClass* GetDeclaringType(Il2CppMetadataGenericContainerHandle handle);
        static Il2CppMetadataGenericParameterHandle GetGenericParameter(Il2CppMetadataGenericContainerHandle handle, uint16_t index);
    };
} /* namespace vm */
} /* namespace il2cpp */
