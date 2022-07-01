#include "il2cpp-config.h"
#include <vector>
#include <map>
#include <limits>
#include "os/Mutex.h"
#include "utils/StringUtils.h"
#include "vm/Class.h"
#include "vm/Image.h"
#include "vm/MetadataCache.h"
#include "vm/StackTrace.h"
#include "vm/Type.h"
#include "utils/HashUtils.h"
#include "utils/Il2CppHashMap.h"
#include "utils/MemoryMappedFile.h"
#include "utils/StringUtils.h"
#include "vm-utils/VmStringUtils.h"

#include "Baselib.h"
#include "Cpp/ReentrantLock.h"

// ==={{ huatuo
#include "os/Atomic.h"
#include "huatuo/metadata/Image.h"
// ===}} huatuo

struct NamespaceAndNamePairHash
{
    size_t operator()(const std::pair<const char*, const char*>& pair) const
    {
        return il2cpp::utils::HashUtils::Combine(il2cpp::utils::StringUtils::Hash(pair.first), il2cpp::utils::StringUtils::Hash(pair.second));
    }
};

struct NamespaceAndNamePairEquals
{
    bool operator()(const std::pair<const char*, const char*>& p1, const std::pair<const char*, const char*>& p2) const
    {
        return !strcmp(p1.first, p2.first) && !strcmp(p1.second, p2.second);
    }
};

struct NamespaceAndNamePairLess
{
    bool operator()(const std::pair<const char*, const char*>& p1, const std::pair<const char*, const char*>& p2) const
    {
        int namespaceCompare = strcmp(p1.first, p2.first);

        if (namespaceCompare < 0)
            return true;

        if (namespaceCompare > 0)
            return false;

        return strcmp(p1.second, p2.second) < 0;
    }
};

struct Il2CppNameToTypeHandleHashTable : public Il2CppHashMap<std::pair<const char*, const char*>, Il2CppMetadataTypeHandle, NamespaceAndNamePairHash, NamespaceAndNamePairEquals>
{
    typedef Il2CppHashMap<std::pair<const char*, const char*>, Il2CppMetadataTypeHandle, NamespaceAndNamePairHash, NamespaceAndNamePairEquals> Base;
    Il2CppNameToTypeHandleHashTable() : Base()
    {
    }
};

namespace il2cpp
{
namespace vm
{
    const Il2CppAssembly* Image::GetAssembly(const Il2CppImage* image)
    {
        return image->assembly;
    }

    typedef il2cpp::vm::StackFrames::const_reverse_iterator StackReverseIterator;

    static bool IsSystemType(Il2CppClass* klass)
    {
        return strcmp(klass->namespaze, "System") == 0 && strcmp(klass->name, "Type") == 0;
    }

    static bool IsSystemReflectionAssembly(Il2CppClass* klass)
    {
        return strcmp(klass->namespaze, "System.Reflection") == 0 && strcmp(klass->name, "Assembly") == 0;
    }

    static StackReverseIterator GetNextImageFromStack(StackReverseIterator first, StackReverseIterator last)
    {
        for (StackReverseIterator it = first; it != last; it++)
        {
            Il2CppClass* klass = it->method->klass;
            if (klass->image != NULL && !IsSystemType(klass) && !IsSystemReflectionAssembly(klass))
            {
                return it;
            }
        }

        return last;
    }

    const Il2CppImage* Image::GetExecutingImage()
    {
        const il2cpp::vm::StackFrames& stack = *StackTrace::GetStackFrames();
        StackReverseIterator imageIt = GetNextImageFromStack(stack.rbegin(), stack.rend());

        if (imageIt != stack.rend())
        {
            return imageIt->method->klass->image;
        }

        // Fallback to corlib if no image is found
        return const_cast<Il2CppImage*>(Image::GetCorlib());
    }

    const Il2CppImage* Image::GetCallingImage()
    {
        const il2cpp::vm::StackFrames& stack = *StackTrace::GetStackFrames();
        StackReverseIterator imageIt = GetNextImageFromStack(stack.rbegin(), stack.rend());

        if (imageIt != stack.rend())
        {
            imageIt = GetNextImageFromStack(++imageIt, stack.rend());

            if (imageIt != stack.rend())
            {
                return imageIt->method->klass->image;
            }
        }

        // Fallback to corlib if no image is found
        return const_cast<Il2CppImage*>(Image::GetCorlib());
    }

    const char * Image::GetName(const Il2CppImage* image)
    {
        return image->name;
    }

    const char * Image::GetFileName(const Il2CppImage* image)
    {
        return image->name;
    }

    const MethodInfo* Image::GetEntryPoint(const Il2CppImage* image)
    {
        return MetadataCache::GetAssemblyEntryPoint(image);
    }

    Il2CppImage* Image::GetCorlib()
    {
        return il2cpp_defaults.corlib;
    }

    static baselib::ReentrantLock s_ClassFromNameMutex;

    static void AddNestedTypesToNametoClassHashTable(const Il2CppImage* image, Il2CppNameToTypeHandleHashTable* hashTable, const char *namespaze, const std::string& parentName, Il2CppMetadataTypeHandle handle)
    {
        std::pair<const char*, const char*> namespaceAndName = MetadataCache::GetTypeNamespaceAndName(handle);

        std::string name = parentName + "/" + namespaceAndName.second;
        char *pName = (char*)IL2CPP_CALLOC(name.size() + 1, sizeof(char));
        strcpy(pName, name.c_str());

        hashTable->insert(std::make_pair(std::make_pair(namespaze, (const char*)pName), handle));

        void *iter = NULL;
        while (Il2CppMetadataTypeHandle nestedClass = MetadataCache::GetNestedTypes(handle, &iter))
            AddNestedTypesToNametoClassHashTable(image, hashTable, namespaze, name, nestedClass);
    }

    static void AddNestedTypesToNametoClassHashTable(const Il2CppImage* image, Il2CppNameToTypeHandleHashTable* table, Il2CppMetadataTypeHandle handle)
    {
        std::pair<const char*, const char*> namespaceAndName = MetadataCache::GetTypeNamespaceAndName(handle);

        void *iter = NULL;
        while (Il2CppMetadataTypeHandle nestedClass = MetadataCache::GetNestedTypes(handle, &iter))
        {
            // ==={{ huatuo
            AddNestedTypesToNametoClassHashTable(image, table, namespaceAndName.first, namespaceAndName.second, nestedClass);
            // ===}} huatuo
        }
    }

// This must be called when the s_ClassFromNameMutex is held.
    // ==={{ huatuo
    static void AddTypeToNametoClassHashTable(const Il2CppImage* image, Il2CppNameToTypeHandleHashTable* table, Il2CppMetadataTypeHandle typeHandle)
    {
        if (typeHandle == NULL)
            return;

        // don't add nested types
        if (MetadataCache::TypeIsNested(typeHandle))
            return;

        if (image != il2cpp_defaults.corlib)
            AddNestedTypesToNametoClassHashTable(image, table, typeHandle);

        table->insert(std::make_pair(MetadataCache::GetTypeNamespaceAndName(typeHandle), typeHandle));
    }
    // ===}} huatuo

    void Image::InitNestedTypes(const Il2CppImage *image)
    {
        for (uint32_t index = 0; index < image->typeCount; index++)
        {
            Il2CppMetadataTypeHandle handle = MetadataCache::GetAssemblyTypeHandle(image, index);

            // don't add nested types
            if (MetadataCache::TypeIsNested(handle))
                return;

            // ==={{ huatuo
            AddNestedTypesToNametoClassHashTable(image, image->nameToClassHashTable, handle);
            // ===}} huatuo
        }

        for (uint32_t index = 0; index < image->exportedTypeCount; index++)
        {
            Il2CppMetadataTypeHandle handle = MetadataCache::GetAssemblyExportedTypeHandle(image, index);

            // don't add nested types
            if (MetadataCache::TypeIsNested(handle))
                return;
            // ==={{ huatuo
            AddNestedTypesToNametoClassHashTable(image, image->nameToClassHashTable, handle);
            // ===}} huatuo
        }
    }

    Il2CppClass* Image::ClassFromName(const Il2CppImage* image, const char* namespaze, const char *name)
    {
        if (!image->nameToClassHashTable)
        {
            os::FastAutoLock lock(&s_ClassFromNameMutex);
            if (!image->nameToClassHashTable)
            {
                // ==={{ huatuo
                // image->nameToClassHashTable = new Il2CppNameToTypeHandleHashTable(); --- il2cpp
                auto nameToClassHashTable = new Il2CppNameToTypeHandleHashTable();
                for (uint32_t index = 0; index < image->typeCount; index++)
                {
                    AddTypeToNametoClassHashTable(image, nameToClassHashTable, MetadataCache::GetAssemblyTypeHandle(image, index));
                }

                for (uint32_t index = 0; index < image->exportedTypeCount; index++)
                {
                    AddTypeToNametoClassHashTable(image, nameToClassHashTable, MetadataCache::GetAssemblyExportedTypeHandle(image, index));
                }
                os::Atomic::FullMemoryBarrier();
                image->nameToClassHashTable = nameToClassHashTable;
                // ===}} huatuo
            }
        }

        Il2CppNameToTypeHandleHashTable::const_iterator iter = image->nameToClassHashTable->find(std::make_pair(namespaze, name));
        if (iter != image->nameToClassHashTable->end())
            return MetadataCache::GetTypeInfoFromHandle(iter->second);

        return NULL;
    }

    void Image::GetTypes(const Il2CppImage* image, bool exportedOnly, TypeVector* target)
    {
        uint32_t typeCount = Image::GetNumTypes(image);

        for (uint32_t sourceIndex = 0; sourceIndex < typeCount; sourceIndex++)
        {
            const Il2CppClass* type = Image::GetType(image, sourceIndex);
            if (strcmp(type->name, "<Module>") == 0)
            {
                continue;
            }

            target->push_back(type);
        }
    }

    uint32_t Image::GetNumTypes(const Il2CppImage* image)
    {
        return image->typeCount;
    }

    const Il2CppClass* Image::GetType(const Il2CppImage* image, AssemblyTypeIndex index)
    {
        IL2CPP_ASSERT(index <= std::numeric_limits<AssemblyTypeIndex>::max());
        return MetadataCache::GetTypeInfoFromHandle(MetadataCache::GetAssemblyTypeHandle(image, index));
    }

    static bool StringsMatch(const char* left, const char* right, bool ignoreCase)
    {
        if (!ignoreCase)
        {
            return strcmp(left, right) == 0;
        }
        else
        {
            utils::VmStringUtils::CaseInsensitiveComparer comparer;
            return comparer(left, right);
        }
    }

    static bool ClassMatches(Il2CppClass* declaringType, Il2CppMetadataTypeHandle typeHandle, const char* namespaze, bool ignoreCase, const char* name)
    {
        if (declaringType != NULL)
        {
            const Il2CppClass* type = MetadataCache::GetTypeInfoFromHandle(typeHandle);
            if (type->declaringType != declaringType)
            {
                return false;
            }
        }

        // ==={{ huatuo
        std::pair<const char*, const char*> namespaceAndName = MetadataCache::GetTypeNamespaceAndName(typeHandle);
        // ===}} huatuo
        return StringsMatch(namespaze, namespaceAndName.first, ignoreCase) && StringsMatch(name, namespaceAndName.second, ignoreCase);
    }

    static Il2CppClass* FindClassMatching(const Il2CppImage* image, const char* namespaze, const char *name, Il2CppClass* declaringType, bool ignoreCase)
    {
        for (uint32_t i = 0; i < image->typeCount; i++)
        {
            Il2CppMetadataTypeHandle typeHandle = MetadataCache::GetAssemblyTypeHandle(image, i);
            if (ClassMatches(declaringType, typeHandle, namespaze, ignoreCase, name))
                return MetadataCache::GetTypeInfoFromHandle(typeHandle);
        }

        return NULL;
    }

    static Il2CppClass* FindExportedClassMatching(const Il2CppImage* image, const char* namespaze, const char *name, Il2CppClass* declaringType, bool ignoreCase)
    {
        for (uint32_t i = 0; i < image->exportedTypeCount; i++)
        {
            Il2CppMetadataTypeHandle typeHandle = MetadataCache::GetAssemblyExportedTypeHandle(image, i);
            if (ClassMatches(declaringType, typeHandle, namespaze, ignoreCase, name))
                return MetadataCache::GetTypeInfoFromHandle(typeHandle);
        }

        return NULL;
    }

    static Il2CppClass* FindNestedType(Il2CppClass* klass, const char* name)
    {
        void* iter = NULL;
        while (Il2CppClass* nestedType = Class::GetNestedTypes(klass, &iter))
        {
            if (!strcmp(name, nestedType->name))
                return nestedType;
        }

        return NULL;
    }

    Il2CppClass* Image::FromTypeNameParseInfo(const Il2CppImage* image, const TypeNameParseInfo &info, bool ignoreCase)
    {
        const char* ns = info.ns().c_str();
        const char* name = info.name().c_str();
        Il2CppClass *parent_class = FindClassMatching(image, ns, name, NULL, ignoreCase);

        if (parent_class == NULL)
        {
            parent_class = FindExportedClassMatching(image, ns, name, NULL, ignoreCase);
            if (parent_class == NULL)
                return NULL;
        }

        std::vector<std::string>::const_iterator it = info.nested().begin();

        while (it != info.nested().end())
        {
            parent_class = FindNestedType(parent_class, (*it).c_str());

            if (parent_class == NULL)
                return NULL;

            ++it;
        }

        return parent_class;
    }

    static baselib::ReentrantLock s_Mutex;
    static std::vector<Image::EmbeddedResourceData> s_CachedResourceData;
    static std::map<Il2CppReflectionAssembly*, void*> s_CachedMemoryMappedResourceFiles;

    void Image::CacheMemoryMappedResourceFile(Il2CppReflectionAssembly* assembly, void* memoryMappedFile)
    {
        os::FastAutoLock lock(&s_Mutex);
        s_CachedMemoryMappedResourceFiles[assembly] = memoryMappedFile;
    }

    void* Image::GetCachedMemoryMappedResourceFile(Il2CppReflectionAssembly* assembly)
    {
        os::FastAutoLock lock(&s_Mutex);
        std::map<Il2CppReflectionAssembly*, void*>::iterator entry = s_CachedMemoryMappedResourceFiles.find(assembly);
        if (entry != s_CachedMemoryMappedResourceFiles.end())
            return entry->second;

        return NULL;
    }

    void Image::CacheResourceData(EmbeddedResourceRecord record, void* data)
    {
        os::FastAutoLock lock(&s_Mutex);
        s_CachedResourceData.push_back(EmbeddedResourceData(record, data));
    }

    void* Image::GetCachedResourceData(const Il2CppImage* image, const std::string& name)
    {
        os::FastAutoLock lock(&s_Mutex);
        for (std::vector<EmbeddedResourceData>::iterator it = s_CachedResourceData.begin(); it != s_CachedResourceData.end(); ++it)
        {
            if (it->record.image == image && it->record.name == name)
                return it->data;
        }

        return NULL;
    }

    void Image::ClearCachedResourceData()
    {
        os::FastAutoLock lock(&s_Mutex);
        for (std::map<Il2CppReflectionAssembly*, void*>::iterator i = s_CachedMemoryMappedResourceFiles.begin(); i != s_CachedMemoryMappedResourceFiles.end(); ++i)
            utils::MemoryMappedFile::Unmap(i->second);

        s_CachedMemoryMappedResourceFiles.clear();
        s_CachedResourceData.clear();
    }
} /* namespace vm */
} /* namespace il2cpp */
