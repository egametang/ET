#include "il2cpp-config.h"

#if !RUNTIME_TINY

#include <stdint.h>
#include "BlobReader.h"
#include "gc/GarbageCollector.h"
#include "metadata/CustomAttributeDataReader.h"
#include "vm/Array.h"
#include "vm/Class.h"
#include "vm/Object.h"
#include "vm/MetadataCache.h"
#include "vm/Reflection.h"
#include "vm/String.h"
#include "utils/MemoryRead.h"
#include "il2cpp-object-internals.h"

const uint8_t kArrayTypeWithSameElements = 0;
const uint8_t kArrayTypeWithDifferentElements = 1;

namespace il2cpp
{
namespace utils
{
    bool BlobReader::GetConstantValueFromBlob(const Il2CppImage* image, Il2CppTypeEnum type, const char* blob, void* value)
    {
        return GetConstantValueFromBlob(image, type, &blob, value, true);
    }

    bool BlobReader::GetConstantValueFromBlob(const Il2CppImage* image, Il2CppTypeEnum type, const char **blob, void *value, bool deserializeManagedObjects)
    {
        switch (type)
        {
            case IL2CPP_TYPE_BOOLEAN:
            case IL2CPP_TYPE_U1:
            case IL2CPP_TYPE_I1:
                *(uint8_t*)value = Read8(blob);
                break;
            case IL2CPP_TYPE_CHAR:
                *(Il2CppChar*)value = ReadChar(blob);
                break;
            case IL2CPP_TYPE_U2:
            case IL2CPP_TYPE_I2:
                *(uint16_t*)value = Read16(blob);
                break;
            case IL2CPP_TYPE_U4:
                *(uint32_t*)value = ReadCompressedUInt32(blob);
                break;
            case IL2CPP_TYPE_I4:
                *(int32_t*)value = ReadCompressedInt32(blob);
                break;
            case IL2CPP_TYPE_U8:
            case IL2CPP_TYPE_I8:
                *(uint64_t*)value = Read64(blob);
                break;
            case IL2CPP_TYPE_R4:
                *(float*)value = ReadFloat(blob);
                break;
            case IL2CPP_TYPE_R8:
                *(double*)value = ReadDouble(blob);
                break;
            case IL2CPP_TYPE_STRING:
            {
                *(void**)value = NULL;
                if (*blob != NULL)
                {
                    // int32_t length followed by non-null terminated utf-8 byte stream
                    int32_t length = ReadCompressedInt32(blob);

                    // A length of -1 is a null string
                    if (length != -1)
                    {
                        if (deserializeManagedObjects)
                        {
                            *(Il2CppString**)value = il2cpp::vm::String::NewLen(*blob, length);
                            il2cpp::gc::GarbageCollector::SetWriteBarrier((void**)value);
                        }
                        *blob += length;
                    }
                }
                break;
            }
            case IL2CPP_TYPE_SZARRAY:
            {
                if (*blob == NULL)
                {
                    *(void**)value = NULL;
                    return true;
                }

                int32_t arrayLen = ReadCompressedInt32(blob);
                if (arrayLen == -1)
                {
                    // A length of -1 is a null array
                    *(void**)value = NULL;
                    return true;
                }

                Il2CppClass* arrayElementClass;
                Il2CppTypeEnum arrayElementType = ReadEncodedTypeEnum(image, blob, &arrayElementClass);
                uint8_t arrayElementsAreDifferent = Read8(blob);

                IL2CPP_ASSERT(arrayElementClass);
                IL2CPP_ASSERT(arrayElementsAreDifferent != kArrayTypeWithDifferentElements || arrayElementType == IL2CPP_TYPE_OBJECT);

                Il2CppArray* arr = NULL;
                if (deserializeManagedObjects)
                    arr = il2cpp::vm::Array::New(arrayElementClass, arrayLen);

                for (int32_t i = 0; i < arrayLen; i++)
                {
                    Il2CppClass* elementClass = NULL;
                    Il2CppTypeEnum elementType = arrayElementType;
                    if (arrayElementsAreDifferent == kArrayTypeWithDifferentElements)
                        elementType = ReadEncodedTypeEnum(image, blob, &elementClass);

                    // Assumption: The array code is only called for custom attribute data
                    il2cpp::metadata::CustomAttributeDataStorage dataBuffer;
                    IL2CPP_ASSERT(arrayElementClass->element_size <= sizeof(il2cpp::metadata::CustomAttributeDataStorage));
                    if (!GetConstantValueFromBlob(image, elementType, blob, &dataBuffer, deserializeManagedObjects))
                        return false;

                    if (deserializeManagedObjects)
                    {
                        if (elementType != arrayElementType)
                        {
                            IL2CPP_ASSERT(arrayElementType == IL2CPP_TYPE_OBJECT);
                            IL2CPP_ASSERT(elementClass);
                            il2cpp_array_setref(arr, i, il2cpp::vm::Object::Box(elementClass, &dataBuffer));
                        }
                        else
                        {
                            il2cpp_array_setrefwithsize(arr, arr->klass->element_size, i, &dataBuffer);
                        }
                    }
                }

                *(Il2CppArray**)value = arr;
                il2cpp::gc::GarbageCollector::SetWriteBarrier((void**)value);

                break;
            }
            case IL2CPP_TYPE_CLASS:
            case IL2CPP_TYPE_OBJECT:
            case IL2CPP_TYPE_GENERICINST:
                IL2CPP_ASSERT(*blob == NULL);
                *(void**)value = NULL;
                break;
            case IL2CPP_TYPE_IL2CPP_TYPE_INDEX:
            {
                TypeIndex typeIndex = ReadCompressedInt32(blob);
                if (typeIndex == -1)
                {
                    *(void**)value = NULL;
                }
                else if (deserializeManagedObjects)
                {
                    Il2CppClass* klass = il2cpp::vm::MetadataCache::GetTypeInfoFromTypeIndex(image, typeIndex);
                    *(Il2CppReflectionType**)value = il2cpp::vm::Reflection::GetTypeObject(&klass->byval_arg);
                    il2cpp::gc::GarbageCollector::SetWriteBarrier((void**)value);
                }
                break;
            }
            default:
                IL2CPP_ASSERT(0);
                return false;
        }

        return true;
    }

    Il2CppTypeEnum BlobReader::ReadEncodedTypeEnum(const Il2CppImage* image, const char** blob, Il2CppClass** klass)
    {
        Il2CppTypeEnum type = (Il2CppTypeEnum)Read8(blob);
        if (type == IL2CPP_TYPE_ENUM)
        {
            TypeIndex enumTypeIndex = ReadCompressedInt32(blob);
            *klass = il2cpp::vm::MetadataCache::GetTypeInfoFromTypeIndex(image, enumTypeIndex);
            type = il2cpp::vm::Class::GetEnumBaseType(*klass)->type;
        }
        else if (type == IL2CPP_TYPE_SZARRAY)
        {
            // Array class is good enough for this call
            // An array with specific element types will be created if needed
            *klass = il2cpp_defaults.array_class;
        }
        else
        {
            *klass = il2cpp::vm::Class::FromIl2CppTypeEnum(type);
        }

        return type;
    }
} /* utils */
} /* il2cpp */

#endif
