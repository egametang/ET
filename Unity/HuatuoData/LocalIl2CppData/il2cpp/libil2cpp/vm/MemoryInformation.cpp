#include "il2cpp-config.h"
#include "MemoryInformation.h"
#include "gc/GarbageCollector.h"
#include "gc/GCHandle.h"
#include "metadata/ArrayMetadata.h"
#include "metadata/GenericMetadata.h"
#include "vm/Assembly.h"
#include "vm/Class.h"
#include "vm/MetadataCache.h"
#include "vm/Type.h"
#include "utils/Memory.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-tabledefs.h"

#include <map>
#include <limits>

namespace il2cpp
{
namespace vm
{
namespace MemoryInformation
{
    struct GatherMetadataContext
    {
        uint32_t currentIndex;
        std::map<Il2CppClass*, uint32_t> allTypes;
    };

    static void GatherMetadataCallback(Il2CppClass* type, void* context)
    {
        if (type->initialized)
        {
            GatherMetadataContext* ctx = static_cast<GatherMetadataContext*>(context);
            ctx->allTypes.insert(std::make_pair(type, ctx->currentIndex++));
        }
    }

    static inline int FindTypeInfoIndexInMap(const std::map<Il2CppClass*, uint32_t>& allTypes, Il2CppClass* typeInfo)
    {
        std::map<Il2CppClass*, uint32_t>::const_iterator it = allTypes.find(typeInfo);

        if (it == allTypes.end())
            return -1;

        return it->second;
    }

    static inline void GatherMetadata(Il2CppMetadataSnapshot& metadata)
    {
        GatherMetadataContext gatherMetadataContext = { 0 };
        const AssemblyVector* allAssemblies = Assembly::GetAllAssemblies();

        for (AssemblyVector::const_iterator it = allAssemblies->begin(); it != allAssemblies->end(); it++)
        {
            const Il2CppImage& image = *(*it)->image;

            for (uint32_t i = 0; i < image.typeCount; i++)
            {
                Il2CppClass* type = MetadataCache::GetTypeInfoFromHandle(MetadataCache::GetAssemblyTypeHandle(&image, i));
                if (type->initialized)
                    gatherMetadataContext.allTypes.insert(std::make_pair(type, gatherMetadataContext.currentIndex++));
            }
        }

        metadata::ArrayMetadata::WalkArrays(GatherMetadataCallback, &gatherMetadataContext);
        metadata::ArrayMetadata::WalkSZArrays(GatherMetadataCallback, &gatherMetadataContext);
        metadata::GenericMetadata::WalkAllGenericClasses(GatherMetadataCallback, &gatherMetadataContext);
        MetadataCache::WalkPointerTypes(GatherMetadataCallback, &gatherMetadataContext);

        const std::map<Il2CppClass*, uint32_t>& allTypes = gatherMetadataContext.allTypes;
        metadata.typeCount = static_cast<uint32_t>(allTypes.size());
        metadata.types = static_cast<Il2CppMetadataType*>(IL2CPP_CALLOC(metadata.typeCount, sizeof(Il2CppMetadataType)));

        for (std::map<Il2CppClass*, uint32_t>::const_iterator it = allTypes.begin(); it != allTypes.end(); it++)
        {
            Il2CppClass* typeInfo = it->first;

            uint32_t index = it->second;
            Il2CppMetadataType& type = metadata.types[index];

            if (typeInfo->rank > 0)
            {
                type.flags = static_cast<Il2CppMetadataTypeFlags>(kArray | (kArrayRankMask & (typeInfo->rank << 16)));
                type.baseOrElementTypeIndex = FindTypeInfoIndexInMap(allTypes, typeInfo->element_class);
            }
            else
            {
                type.flags = (typeInfo->byval_arg.valuetype || typeInfo->byval_arg.type == IL2CPP_TYPE_PTR) ? kValueType : kNone;
                type.fieldCount = 0;

                if (typeInfo->field_count > 0)
                {
                    type.fields = static_cast<Il2CppMetadataField*>(IL2CPP_CALLOC(typeInfo->field_count, sizeof(Il2CppMetadataField)));

                    for (int i = 0; i < typeInfo->field_count; i++)
                    {
                        Il2CppMetadataField& field = metadata.types[index].fields[type.fieldCount];
                        FieldInfo* fieldInfo = typeInfo->fields + i;
                        field.typeIndex = FindTypeInfoIndexInMap(allTypes, Class::FromIl2CppType(fieldInfo->type));

                        // This will happen if fields type is not initialized
                        // It's OK to skip it, because it means the field is guaranteed to be null on any object
                        if (field.typeIndex == -1)
                            continue;

                        //literals have no actual storage, and are not relevant in this context.
                        if ((fieldInfo->type->attrs & FIELD_ATTRIBUTE_LITERAL) != 0)
                            continue;

                        field.isStatic = (fieldInfo->type->attrs & FIELD_ATTRIBUTE_STATIC) != 0;
                        field.offset = fieldInfo->offset;
                        field.name = fieldInfo->name;
                        type.fieldCount++;
                    }
                }

                type.staticsSize = typeInfo->static_fields_size;

                if (type.staticsSize > 0 && typeInfo->static_fields != NULL)
                {
                    type.statics = static_cast<uint8_t*>(IL2CPP_MALLOC(type.staticsSize));
                    memcpy(type.statics, typeInfo->static_fields, type.staticsSize);
                }

                Il2CppClass* baseType = Class::GetParent(typeInfo);
                type.baseOrElementTypeIndex = baseType != NULL ? FindTypeInfoIndexInMap(allTypes, baseType) : -1;
            }

            type.assemblyName = typeInfo->image->assembly->aname.name;

            std::string typeName = Type::GetName(&typeInfo->byval_arg, IL2CPP_TYPE_NAME_FORMAT_IL);
            type.name = static_cast<char*>(IL2CPP_CALLOC(typeName.length() + 1, sizeof(char)));
            memcpy(type.name, typeName.c_str(), typeName.length() + 1);

            type.typeInfoAddress = reinterpret_cast<uint64_t>(typeInfo);
            type.size = (typeInfo->byval_arg.valuetype) != 0 ? (typeInfo->instance_size - sizeof(Il2CppObject)) : typeInfo->instance_size;
        }
    }

    struct SectionIterationContext
    {
        Il2CppManagedMemorySection* currentSection;
    };

    static void AllocateMemoryForSection(void* context, void* sectionStart, void* sectionEnd)
    {
        SectionIterationContext* ctx = static_cast<SectionIterationContext*>(context);
        Il2CppManagedMemorySection& section = *ctx->currentSection;

        section.sectionStartAddress = reinterpret_cast<uint64_t>(sectionStart);

        ptrdiff_t sectionSize = static_cast<uint8_t*>(sectionEnd) - static_cast<uint8_t*>(sectionStart);

        if (sizeof(void*) > 4) // This assert is only valid on 64-bit
            IL2CPP_ASSERT(sectionSize <= static_cast<ptrdiff_t>(std::numeric_limits<uint32_t>::max()));

        section.sectionSize = static_cast<uint32_t>(sectionSize);
        section.sectionBytes = static_cast<uint8_t*>(IL2CPP_MALLOC(section.sectionSize));

        ctx->currentSection++;
    }

    static void CopyHeapSection(void* context, void* sectionStart, void* sectionEnd)
    {
        SectionIterationContext* ctx = static_cast<SectionIterationContext*>(context);
        Il2CppManagedMemorySection& section = *ctx->currentSection;

        IL2CPP_ASSERT(section.sectionStartAddress == reinterpret_cast<uint64_t>(sectionStart));
        IL2CPP_ASSERT(section.sectionSize == static_cast<uint8_t*>(sectionEnd) - static_cast<uint8_t*>(sectionStart));
        memcpy(section.sectionBytes, sectionStart, section.sectionSize);

        ctx->currentSection++;
    }

    static void* CaptureHeapInfo(void* voidManagedHeap)
    {
        Il2CppManagedHeap& heap = *(Il2CppManagedHeap*)voidManagedHeap;

        heap.sectionCount = static_cast<uint32_t>(il2cpp::gc::GarbageCollector::GetSectionCount());
        heap.sections = static_cast<Il2CppManagedMemorySection*>(IL2CPP_CALLOC(heap.sectionCount, sizeof(Il2CppManagedMemorySection)));

        SectionIterationContext iterationContext = { heap.sections };
        il2cpp::gc::GarbageCollector::ForEachHeapSection(&iterationContext, AllocateMemoryForSection);

        return NULL;
    }

    static void FreeIL2CppManagedHeap(Il2CppManagedHeap& heap)
    {
        for (uint32_t i = 0; i < heap.sectionCount; i++)
        {
            IL2CPP_FREE(heap.sections[i].sectionBytes);
        }

        IL2CPP_FREE(heap.sections);
    }

    struct VerifyHeapSectionStillValidIterationContext
    {
        Il2CppManagedMemorySection* currentSection;
        bool wasValid;
    };

    static void VerifyHeapSectionIsStillValid(void* context, void* sectionStart, void* sectionEnd)
    {
        VerifyHeapSectionStillValidIterationContext* iterationContext = (VerifyHeapSectionStillValidIterationContext*)context;
        if (iterationContext->currentSection->sectionSize != static_cast<uint8_t*>(sectionEnd) - static_cast<uint8_t*>(sectionStart))
            iterationContext->wasValid = false;
        else if (iterationContext->currentSection->sectionStartAddress != reinterpret_cast<uint64_t>(sectionStart))
            iterationContext->wasValid = false;

        iterationContext->currentSection++;
    }

    static bool IsIL2CppManagedHeapStillValid(Il2CppManagedHeap& heap)
    {
        if (heap.sectionCount != static_cast<uint32_t>(il2cpp::gc::GarbageCollector::GetSectionCount()))
            return false;

        VerifyHeapSectionStillValidIterationContext iterationContext = { heap.sections, true };
        il2cpp::gc::GarbageCollector::ForEachHeapSection(&iterationContext, VerifyHeapSectionIsStillValid);

        return iterationContext.wasValid;
    }

// The difficulty in capturing the managed snapshot is that we need to do quite some work with the world stopped,
// to make sure that our snapshot is "valid", and didn't change as we were copying it. However, stopping the world,
// makes it so you cannot take any lock or allocations. We deal with it like this:
//
// 1) We take note of the amount of heap sections and their sizes, and we allocate memory to copy them into.
// 2) We stop the world.
// 3) We check if the amount of heapsections and their sizes didn't change in the mean time. If they did, try again.
// 4) Now, with the world still stopped, we memcpy() the memory from the real heapsections, into the memory that we
//    allocated for their copies.
// 5) Start the world again.

    static inline void CaptureManagedHeap(Il2CppManagedHeap& heap)
    {
        for (;;)
        {
            il2cpp::gc::GarbageCollector::CallWithAllocLockHeld(CaptureHeapInfo, &heap);

            il2cpp::gc::GarbageCollector::StopWorld();

            if (IsIL2CppManagedHeapStillValid(heap))
                break;

            il2cpp::gc::GarbageCollector::StartWorld();

            FreeIL2CppManagedHeap(heap);
        }

        SectionIterationContext iterationContext = { heap.sections };
        il2cpp::gc::GarbageCollector::ForEachHeapSection(&iterationContext, CopyHeapSection);

        il2cpp::gc::GarbageCollector::StartWorld();
    }

    struct GCHandleTargetIterationContext
    {
        std::vector<Il2CppObject*> managedObjects;
    };

    static void GCHandleIterationCallback(Il2CppObject* managedObject, void* context)
    {
        GCHandleTargetIterationContext* ctx = static_cast<GCHandleTargetIterationContext*>(context);
        ctx->managedObjects.push_back(managedObject);
    }

    static inline void CaptureGCHandleTargets(Il2CppGCHandles& gcHandles)
    {
        GCHandleTargetIterationContext gcHandleTargetIterationContext;
        il2cpp::gc::GCHandle::WalkStrongGCHandleTargets(GCHandleIterationCallback, &gcHandleTargetIterationContext);

        const std::vector<Il2CppObject*>& trackedObjects = gcHandleTargetIterationContext.managedObjects;
        gcHandles.trackedObjectCount = static_cast<uint32_t>(trackedObjects.size());
        gcHandles.pointersToObjects = static_cast<uint64_t*>(IL2CPP_CALLOC(gcHandles.trackedObjectCount, sizeof(uint64_t)));

        for (uint32_t i = 0; i < gcHandles.trackedObjectCount; i++)
            gcHandles.pointersToObjects[i] = reinterpret_cast<uint64_t>(trackedObjects[i]);
    }

    void FillRuntimeInformation(Il2CppRuntimeInformation& runtimeInfo)
    {
        runtimeInfo.pointerSize = static_cast<uint32_t>(sizeof(void*));
        runtimeInfo.objectHeaderSize = static_cast<uint32_t>(sizeof(Il2CppObject));
        runtimeInfo.arrayHeaderSize = static_cast<uint32_t>(kIl2CppSizeOfArray);
        runtimeInfo.arraySizeOffsetInHeader = kIl2CppOffsetOfArrayLength;
        runtimeInfo.arrayBoundsOffsetInHeader = kIl2CppOffsetOfArrayBounds;
        runtimeInfo.allocationGranularity = static_cast<uint32_t>(2 * sizeof(void*));
    }

    struct il2cpp_heap_chunk
    {
        void* start;
        uint32_t size;
    };

    void ReportIL2CppClasses(ClassReportFunc callback, void* context)
    {
        const AssemblyVector* allAssemblies = Assembly::GetAllAssemblies();

        for (AssemblyVector::const_iterator it = allAssemblies->begin(); it != allAssemblies->end(); it++)
        {
            const Il2CppImage& image = *(*it)->image;

            for (uint32_t i = 0; i < image.typeCount; i++)
            {
                Il2CppClass* type = MetadataCache::GetTypeInfoFromHandle(MetadataCache::GetAssemblyTypeHandle(&image, i));
                if (type->initialized)
                    callback(type, context);
            }
        }

        metadata::ArrayMetadata::WalkArrays(callback, context);
        metadata::ArrayMetadata::WalkSZArrays(callback, context);
        metadata::GenericMetadata::WalkAllGenericClasses(callback, context);
        MetadataCache::WalkPointerTypes(callback, context);
    }

    void ReportGcHeapSection(void * context, void * start, void * end)
    {
        il2cpp_heap_chunk chunk;
        chunk.start = start;
        //todo: change back to size_t once we change the memory profiler format and mono to use size_t for reporting chunk size
        chunk.size = (uint32_t)((uint8_t *)end - (uint8_t *)start);
        IterationContext* ctxPtr = reinterpret_cast<IterationContext*>(context);
        ctxPtr->callback(&chunk, ctxPtr->userData);
    }

    void ReportGcHandleTarget(Il2CppObject * obj, void * context)
    {
        IterationContext* ctxPtr = reinterpret_cast<IterationContext*>(context);
        ctxPtr->callback(obj, ctxPtr->userData);
    }

    Il2CppManagedMemorySnapshot* CaptureManagedMemorySnapshot()
    {
        Il2CppManagedMemorySnapshot* snapshot = static_cast<Il2CppManagedMemorySnapshot*>(IL2CPP_MALLOC_ZERO(sizeof(Il2CppManagedMemorySnapshot)));

        GatherMetadata(snapshot->metadata);
        CaptureManagedHeap(snapshot->heap);
        CaptureGCHandleTargets(snapshot->gcHandles);
        FillRuntimeInformation(snapshot->runtimeInformation);

        return snapshot;
    }

    void FreeCapturedManagedMemorySnapshot(Il2CppManagedMemorySnapshot* snapshot)
    {
        FreeIL2CppManagedHeap(snapshot->heap);

        IL2CPP_FREE(snapshot->gcHandles.pointersToObjects);

        Il2CppMetadataSnapshot& metadata = snapshot->metadata;

        for (uint32_t i = 0; i < metadata.typeCount; i++)
        {
            if ((metadata.types[i].flags & kArray) == 0)
            {
                IL2CPP_FREE(metadata.types[i].fields);
                IL2CPP_FREE(metadata.types[i].statics);
            }

            IL2CPP_FREE(metadata.types[i].name);
        }

        IL2CPP_FREE(metadata.types);
        IL2CPP_FREE(snapshot);
    }
} // namespace MemoryInformation
} // namespace vm
} // namespace il2cpp
