#include "CustomAttributeDataReader.h"
#include "il2cpp-metadata.h"
#include "gc/WriteBarrier.h"
#include "utils/MemoryRead.h"
#include "vm-utils/BlobReader.h"
#include "vm/Class.h"
#include "vm/Exception.h"
#include "vm/GlobalMetadata.h"
#include "vm/MetadataCache.h"

// Custom attribute metadata format
//
// Custom attribute data is tightly packed and is not stored aligned
// it must be read with the helpers in MemoryRead
//
// Header:
// 1 Compressed uint32: Count of attributes types
// n uint32:            Attribute constructor indexes
//
// Argument data immediate follows the header
// There is no size data stored for arguments they must be serialized
// out as they are read.  This relies the writing code exactly matching
// the reading code.  Or else data will be read at the wrong offsets.
//
// Argument data
// 1 Compressed uint32:         Count of constructor arguments
//  n Blob data, variable sized:   Argument data
// 1 Compressed uint32:         Count of field arguments
//  n Blob data, variable sized:   Field argument data
//     Each field data ends with a compressed int32 of the field index on the type,
//     If the field index is nengative, a compressed uint32_t with the declaring type index follows
// 1 Compressed uint32:         Count of property arguments
//  n Blob data, variable sized:   Property argument data
//     Each property data ends with a compressed int32 of the property index on the type
//     If the property index is nengative, a compressed uint32_t with the declaring type index follows

// An example format is:
//
// 0x02         - Count of custom attribute constructors (compressed uint32_t)
// 0x0010023f   - Method definition index for ctor1
// 0x02001fc1   - Method definition index for ctor2
// 0x02         - Constructor argument count for ctor1 (compressed uint32_t)
// 0x04 (2)     - argument 1 type code (compressed int32_t)
// 0x00         - Field for ctor1 (compressed uint32_t)
// 0x01         - Property count for ctor1 (comprrressed uint32_t)
// ....         - argument 1 data
// 0x55         - property type code (enum) (comprssed uint32_t)
// 0x023F       - type index for enum type (compressed uint32_t))
// ....         - property 1 data
// 0x02         - Constructor argument count for ctor2 (compressed uint32_t)
// 0x02         - Field argument count for ctor2 (compressed uint32_t)
// 0x00         - Property count for ctor2
// 0x03         - argument 1 type code (compressed uint32_t)
// ....         - argument 1 data
// 0x04         - argument 2 type code (compressed uint32_t)
// ....         - argument 2 data
// 0x04         - field 1 type code (compressed uint32_t)
// ....         - field 1 data
// 0x02 (1)     - field 1 field index (compressed int32_t)
// 0x04         - field 2 type code (compressed uint32_t)
// ....         - field 2 data
// 0x03 (-1)    - field 2 field index (compressed int32_t)
// 0x023E       - field 2 declaring type index (compressed int32_t)
// [Start of next custom attribute data]

static void SetInvalidDataException(Il2CppException** exc)
{
    il2cpp::gc::WriteBarrier::GenericStore(exc, il2cpp::vm::Exception::GetCustomAttributeFormatException("Binary format of the specified custom attribute was invalid."));
}

static bool ReadAttributeDataValue(const Il2CppImage* image, const char** buffer, il2cpp::metadata::CustomAttributeArgument* arg, Il2CppException** exc, bool deserializedManagedObjects)
{
    const Il2CppTypeEnum type = il2cpp::utils::BlobReader::ReadEncodedTypeEnum(image, buffer, &arg->klass);

    if (!il2cpp::utils::BlobReader::GetConstantValueFromBlob(image, type, buffer, &arg->data, deserializedManagedObjects))
    {
        SetInvalidDataException(exc);
        return false;
    }

    if (deserializedManagedObjects && type == IL2CPP_TYPE_SZARRAY && arg->data.obj != NULL)
    {
        // For arrays get the actual array class, not just System.Array
        arg->klass = ((Il2CppArray*)arg->data.obj)->klass;
    }

    return true;
}

namespace il2cpp
{
namespace metadata
{
    CustomAttributeDataReader::CustomAttributeDataReader(const void* buffer, const void* bufferEnd) :
        bufferStart((const char*)buffer), bufferEnd((const char*)bufferEnd)
    {
        if (bufferStart != NULL)
            count = utils::ReadCompressedUInt32(&bufferStart);
        else
            count = 0;
    }

    // private, used by CustomAttributeDataReader::ReadCustomAttributeData(const MethodInfo* ctor, const void* dataStart, uint32_t dataLength, CustomAttributeData* data, Il2CppException** exc)
    CustomAttributeDataReader::CustomAttributeDataReader(const char* dataStart, uint32_t dataLength) :
        bufferStart(dataStart), bufferEnd(dataStart + dataLength), count(0)
    {
    }

    uint32_t CustomAttributeDataReader::GetCount()
    {
        return count;
    }

    CustomAttributeCtorIterator CustomAttributeDataReader::GetCtorIterator()
    {
        return CustomAttributeCtorIterator(bufferStart);
    }

    CustomAttributeDataIterator CustomAttributeDataReader::GetDataIterator()
    {
        return CustomAttributeDataIterator(bufferStart, GetDataBufferStart());
    }

    const char* CustomAttributeDataReader::GetDataBufferStart()
    {
        return (const char*)(((uint32_t*)bufferStart) + count);
    }

    bool CustomAttributeDataReader::IterateAttributeCtors(const Il2CppImage* image, const MethodInfo** attributeCtor, CustomAttributeCtorIterator* iter)
    {
        if (iter->ctorBuffer < GetDataBufferStart())
        {
            MethodIndex ctorIndex = utils::Read32(&iter->ctorBuffer);
            *attributeCtor = il2cpp::vm::MetadataCache::GetMethodInfoFromMethodDefinitionIndex(image, ctorIndex);
            return true;
        }

        *attributeCtor = NULL;
        return false;
    }

    bool CustomAttributeDataReader::ReadLazyCustomAttributeData(const Il2CppImage* image, LazyCustomAttributeData* data, CustomAttributeDataIterator* iter, Il2CppException** exc)
    {
        if (!IterateAttributeCtors(image, &data->ctor, &iter->ctorIter))
            return false;

        data->dataStart = (void*)iter->dataBuffer;

        CustomAttributeReaderVisitor visitor;
        if (!VisitCustomAttributeDataImpl(image, data->ctor, iter, &visitor, exc, false))
            return false;

        data->dataLength = (uint32_t)((char*)iter->dataBuffer - (char*)data->dataStart);

        return true;
    }

    bool CustomAttributeDataReader::VisitCustomAttributeData(const Il2CppImage* image, const MethodInfo* ctor, const void* dataStart, uint32_t dataLength, CustomAttributeReaderVisitor* visitor, Il2CppException** exc)
    {
        CustomAttributeDataReader reader = CustomAttributeDataReader((const char*)dataStart, dataLength);
        CustomAttributeDataIterator iter = CustomAttributeDataIterator(NULL, reader.bufferStart);
        return reader.VisitCustomAttributeDataImpl(image, ctor, &iter, visitor, exc, true);
    }

    bool CustomAttributeDataReader::VisitCustomAttributeData(const Il2CppImage* image, CustomAttributeDataIterator* iter, CustomAttributeReaderVisitor* visitor, Il2CppException** exc)
    {
        const MethodInfo* ctor;

        if (!IterateAttributeCtors(image, &ctor, &iter->ctorIter))
            return false;

        return VisitCustomAttributeDataImpl(image, ctor, iter, visitor, exc, true);
    }

    static std::tuple<const Il2CppClass*, int32_t> ReadCustomAttributeNamedArgumentClassAndIndex(const char** dataBuffer, const Il2CppClass* attrClass)
    {
        int32_t memberIndex = utils::ReadCompressedInt32(dataBuffer);
        if (memberIndex >= 0)
            return std::make_tuple(attrClass, memberIndex);

        memberIndex = -(memberIndex + 1);

        TypeDefinitionIndex typeIndex = utils::ReadCompressedUInt32(dataBuffer);
        Il2CppClass* declaringClass = il2cpp::vm::GlobalMetadata::GetTypeInfoFromTypeDefinitionIndex(typeIndex);

        IL2CPP_ASSERT(declaringClass == attrClass || il2cpp::vm::Class::IsSubclassOf(const_cast<Il2CppClass*>(attrClass), declaringClass, false));

        return std::make_tuple(declaringClass, memberIndex);
    }

    bool CustomAttributeDataReader::VisitCustomAttributeDataImpl(const Il2CppImage* image, const MethodInfo* ctor, CustomAttributeDataIterator* iter, CustomAttributeReaderVisitor* visitor, Il2CppException** exc, bool deserializedManagedObjects)
    {
        il2cpp::gc::WriteBarrier::GenericStoreNull(exc);

        const Il2CppClass* attrClass = ctor->klass;

        uint32_t argumentCount = utils::ReadCompressedUInt32(&iter->dataBuffer);
        IL2CPP_ASSERT(iter->dataBuffer <= bufferEnd);
        uint32_t fieldCount = utils::ReadCompressedUInt32(&iter->dataBuffer);
        IL2CPP_ASSERT(iter->dataBuffer <= bufferEnd);
        uint32_t propertyCount = utils::ReadCompressedUInt32(&iter->dataBuffer);
        IL2CPP_ASSERT(iter->dataBuffer <= bufferEnd);

        if (iter->dataBuffer > bufferEnd)
        {
            // This should never happen
            IL2CPP_ASSERT(false);
            SetInvalidDataException(exc);
            return false;
        }

        visitor->VisitArgumentSizes(argumentCount, fieldCount, propertyCount);

        // CustomAttributeArgument may contain GC allocated types
        // So it either needs to be allocated on the stack or on the GC heap
        // Since these are arguments that would be passed to a method call, we assume that we're safe to stack allocate them
        CustomAttributeArgument* args = (CustomAttributeArgument*)alloca(argumentCount * sizeof(CustomAttributeArgument));

        for (uint32_t i = 0; i < argumentCount; i++)
        {
            if (!ReadAttributeDataValue(image, &iter->dataBuffer, args + i, exc, deserializedManagedObjects))
                return false;

            IL2CPP_ASSERT(iter->dataBuffer <= bufferEnd);
            visitor->VisitArgument(args[i], i);
        }

        visitor->VisitCtor(ctor, args, argumentCount);

        for (uint32_t i = 0; i < fieldCount; i++)
        {
            CustomAttributeFieldArgument field = { 0 };
            if (!ReadAttributeDataValue(image, &iter->dataBuffer, &field.arg, exc, deserializedManagedObjects))
                return false;

            const Il2CppClass* klass;
            TypeFieldIndex fieldIndex;
            std::tie(klass, fieldIndex) = ReadCustomAttributeNamedArgumentClassAndIndex(&iter->dataBuffer, attrClass);

            IL2CPP_ASSERT(iter->dataBuffer <= bufferEnd);
            IL2CPP_ASSERT(fieldIndex < klass->field_count);

            field.field = &klass->fields[fieldIndex];
            visitor->VisitField(field, i);
        }

        for (uint32_t i = 0; i < propertyCount; i++)
        {
            CustomAttributePropertyArgument propArg = { 0 };
            if (!ReadAttributeDataValue(image, &iter->dataBuffer, &propArg.arg, exc, deserializedManagedObjects))
                return false;

            const Il2CppClass* klass;
            TypePropertyIndex propertyIndex;
            std::tie(klass, propertyIndex) = ReadCustomAttributeNamedArgumentClassAndIndex(&iter->dataBuffer, attrClass);

            IL2CPP_ASSERT(iter->dataBuffer <= bufferEnd);
            IL2CPP_ASSERT(propertyIndex < klass->property_count);

            propArg.prop = &klass->properties[propertyIndex];
            visitor->VisitProperty(propArg, i);
        }

        return true;
    }
}
}
