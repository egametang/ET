#pragma once

#include <stdint.h>
#include <vector>
#include "metadata/Il2CppTypeVector.h"

namespace il2cpp
{
namespace metadata
{
    struct SizeAndAlignment
    {
        size_t size;
        uint8_t alignment;
        uint8_t naturalAlignment;
    };

    class FieldLayout
    {
    public:
        struct FieldLayoutData
        {
            std::vector<size_t> FieldOffsets;
            size_t classSize;
            size_t actualClassSize;
            uint8_t minimumAlignment;
            uint8_t naturalAlignment;
        };

        static void LayoutFields(size_t parentSize, size_t actualParentSize, size_t parentAlignment, uint8_t packing, const Il2CppTypeVector& fieldTypes, FieldLayoutData& data);
        static SizeAndAlignment GetTypeSizeAndAlignment(const Il2CppType* type);
    };
} /* namespace metadata */
} /* namespace il2cpp */
