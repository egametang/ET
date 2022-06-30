#pragma once

#include "il2cpp-config.h"

struct Il2CppMetadataField
{
    uint32_t offset;
    uint32_t typeIndex;
    const char* name;
    bool isStatic;
};

enum Il2CppMetadataTypeFlags
{
    kNone = 0,
    kValueType = 1 << 0,
    kArray = 1 << 1,
    kArrayRankMask = 0xFFFF0000
};

struct Il2CppMetadataType
{
    Il2CppMetadataTypeFlags flags;  // If it's an array, rank is encoded in the upper 2 bytes
    Il2CppMetadataField* fields;
    uint32_t fieldCount;
    uint32_t staticsSize;
    uint8_t* statics;
    uint32_t baseOrElementTypeIndex;
    char* name;
    const char* assemblyName;
    uint64_t typeInfoAddress;
    uint32_t size;
};

struct Il2CppMetadataSnapshot
{
    uint32_t typeCount;
    Il2CppMetadataType* types;
};

struct Il2CppManagedMemorySection
{
    uint64_t sectionStartAddress;
    uint32_t sectionSize;
    uint8_t* sectionBytes;
};

struct Il2CppManagedHeap
{
    uint32_t sectionCount;
    Il2CppManagedMemorySection* sections;
};

struct Il2CppStacks
{
    uint32_t stackCount;
    Il2CppManagedMemorySection* stacks;
};

struct NativeObject
{
    uint32_t gcHandleIndex;
    uint32_t size;
    uint32_t instanceId;
    uint32_t classId;
    uint32_t referencedNativeObjectIndicesCount;
    uint32_t* referencedNativeObjectIndices;
};

struct Il2CppGCHandles
{
    uint32_t trackedObjectCount;
    uint64_t* pointersToObjects;
};

struct Il2CppRuntimeInformation
{
    uint32_t pointerSize;
    uint32_t objectHeaderSize;
    uint32_t arrayHeaderSize;
    uint32_t arrayBoundsOffsetInHeader;
    uint32_t arraySizeOffsetInHeader;
    uint32_t allocationGranularity;
};

struct Il2CppManagedMemorySnapshot
{
    Il2CppManagedHeap heap;
    Il2CppStacks stacks;
    Il2CppMetadataSnapshot metadata;
    Il2CppGCHandles gcHandles;
    Il2CppRuntimeInformation runtimeInformation;
    void* additionalUserInformation;
};

namespace il2cpp
{
namespace vm
{
namespace MemoryInformation
{
    typedef void(*ClassReportFunc)(Il2CppClass* klass, void* context);
    typedef void(*DataReportFunc)(void* data, void* context);

    struct IterationContext
    {
        DataReportFunc callback;
        void* userData;
    };

    void ReportIL2CppClasses(ClassReportFunc callback, void* context);
    void ReportGcHeapSection(void* context, void* start, void* end);
    void ReportGcHandleTarget(Il2CppObject* obj, void* context);
    Il2CppManagedMemorySnapshot* CaptureManagedMemorySnapshot();
    void FreeCapturedManagedMemorySnapshot(Il2CppManagedMemorySnapshot* snapshot);
}
}
}
