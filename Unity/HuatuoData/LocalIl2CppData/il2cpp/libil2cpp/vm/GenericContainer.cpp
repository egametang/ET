#include "il2cpp-config.h"
#include "GenericContainer.h"
#include "MetadataCache.h"

namespace il2cpp
{
namespace vm
{
    Il2CppClass* GenericContainer::GetDeclaringType(Il2CppMetadataGenericContainerHandle handle)
    {
        return MetadataCache::GetContainerDeclaringType(handle);
    }

    Il2CppMetadataGenericParameterHandle GenericContainer::GetGenericParameter(Il2CppMetadataGenericContainerHandle handle, uint16_t index)
    {
        return MetadataCache::GetGenericParameterFromIndex(handle, index);
    }
} /* namespace vm */
} /* namespace il2cpp */
