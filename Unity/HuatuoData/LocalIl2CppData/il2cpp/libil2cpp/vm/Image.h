#pragma once

#include <stdint.h>
#include <string>
#include <vector>
#include "il2cpp-config.h"

struct Il2CppClass;
struct MethodInfo;
struct Il2CppAssembly;
struct Il2CppDelegate;
struct Il2CppImage;
struct Il2CppType;
struct Il2CppGenericContext;
struct Il2CppGenericContainer;
struct Il2CppReflectionAssembly;
struct Il2CppArray;
class AssemblyVector;

namespace il2cpp
{
namespace vm
{
    typedef std::vector<const Il2CppClass*> TypeVector;

    class TypeNameParseInfo;

    struct EmbeddedResourceRecord
    {
        EmbeddedResourceRecord(const Il2CppImage* image, const std::string& name, uint32_t offset, uint32_t size)
            : image(image), name(name), offset(offset), size(size)
        {}

        const Il2CppImage* image;
        std::string name;
        uint32_t offset;
        uint32_t size;
    };

    class LIBIL2CPP_CODEGEN_API Image
    {
// exported
    public:
        static Il2CppImage* GetCorlib();

    public:
        static const char * GetName(const Il2CppImage* image);
        static const char * GetFileName(const Il2CppImage* image);
        static const Il2CppAssembly* GetAssembly(const Il2CppImage* image);
        static const MethodInfo* GetEntryPoint(const Il2CppImage* image);
        static const Il2CppImage* GetExecutingImage();
        static const Il2CppImage* GetCallingImage();
        static uint32_t GetNumTypes(const Il2CppImage* image);
        static const Il2CppClass* GetType(const Il2CppImage* image, AssemblyTypeIndex index);
        static Il2CppClass* FromTypeNameParseInfo(const Il2CppImage* image, const TypeNameParseInfo &info, bool ignoreCase);
        static Il2CppClass* ClassFromName(const Il2CppImage* image, const char* namespaze, const char *name);
        static void GetTypes(const Il2CppImage* image, bool exportedOnly, TypeVector* target);

        struct EmbeddedResourceData
        {
            EmbeddedResourceData(EmbeddedResourceRecord record, void* data)
                : record(record), data(data)
            {}

            EmbeddedResourceRecord record;
            void* data;
        };

        static void CacheMemoryMappedResourceFile(Il2CppReflectionAssembly* assembly, void* memoryMappedFile);
        static void* GetCachedMemoryMappedResourceFile(Il2CppReflectionAssembly* assembly);
        static void CacheResourceData(EmbeddedResourceRecord record, void* data);
        static void* GetCachedResourceData(const Il2CppImage* image, const std::string& name);
        static void ClearCachedResourceData();
        static void InitNestedTypes(const Il2CppImage *image);
    };
} /* namespace vm */
} /* namespace il2cpp */
