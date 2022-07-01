#include "il2cpp-config.h"
#include "il2cpp-runtime-stats.h"
#include "os/Mutex.h"
#include "vm/Class.h"
#include "vm/GenericClass.h"
#include "vm/Image.h"
#include "vm/MetadataLock.h"
#include "vm/Type.h"
#include "metadata/ArrayMetadata.h"
#include "metadata/GenericMetadata.h"
#include "metadata/GenericMethod.h"
#include "metadata/Il2CppGenericClassHash.h"
#include "metadata/Il2CppGenericClassCompare.h"
#include "metadata/Il2CppGenericInstCompare.h"
#include "metadata/Il2CppGenericInstHash.h"
#include "metadata/Il2CppTypeCompare.h"
#include "metadata/Il2CppTypeHash.h"
#include "vm/MetadataAlloc.h"
#include "vm/MetadataCache.h"
#include "vm/Runtime.h"
#include "utils/Memory.h"
#include "utils/Il2CppHashMap.h"
#include "utils/StringUtils.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-tabledefs.h"
#include <vector>
#include <limits>

using namespace il2cpp::vm;
using il2cpp::os::FastAutoLock;
using il2cpp::utils::StringUtils;
using il2cpp::vm::MetadataCache;

using il2cpp::vm::MetadataCalloc;
using il2cpp::vm::MetadataMalloc;

using std::vector;
using std::pair;


const size_t kImplicitArrayInterfaceCount = 5;

namespace il2cpp
{
namespace metadata
{
    static const char* GetArrayName(const char* elementClassName, uint32_t rank, bool bounded)
    {
        std::string name;
        name += elementClassName;
        name += "[";
        for (uint32_t i = 1; i < rank; i++)
            name += ",";
        if (bounded)
            name += "*";
        name += "]";

        return StringUtils::StringDuplicate(name.c_str());
    }

    static MethodInfo* ConstructArrayMethod(Il2CppClass* declaringType, const char* name, const Il2CppType* returnType, uint8_t parameterCount, const Il2CppType** parameterTypes)
    {
        MethodInfo* method = (MethodInfo*)MetadataCalloc(1, sizeof(MethodInfo));
        method->klass = declaringType;
        method->flags = METHOD_ATTRIBUTE_PUBLIC;
        method->iflags = METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL;
        method->name = name;
        method->slot = kInvalidIl2CppMethodSlot;
        method->return_type = returnType;
        method->parameters_count = parameterCount;
        ParameterInfo* parameters = (ParameterInfo*)MetadataCalloc(parameterCount, sizeof(ParameterInfo));
        for (uint8_t i = 0; i < parameterCount; i++)
        {
            parameters[i].position = i;
            parameters[i].parameter_type = parameterTypes[i];
            parameters[i].name = NULL;
        }
        method->parameters = parameters;

        if (!strcmp(".ctor", name))
        {
            method->flags |= METHOD_ATTRIBUTE_RT_SPECIAL_NAME | METHOD_ATTRIBUTE_SPECIAL_NAME;
        }
        else
        {
            method->iflags |= METHOD_IMPL_ATTRIBUTE_RUNTIME;
        }

        ++il2cpp_runtime_stats.method_count;

        return method;
    }

    struct GenericArrayMethod
    {
        std::string name;
        const MethodInfo* method;
        const MethodInfo* interfaceMethodDefinition;
    };

    typedef vector<GenericArrayMethod> GenericArrayMethods;
    static GenericArrayMethods s_GenericArrayMethods;

    static size_t GetArrayGenericMethodsCount();
    static void PopulateArrayGenericMethods(Il2CppClass* klass, uint16_t offset, const GenericArrayMethods& genericArrayMethods);

    static void CollectImplicitArrayInterfaces(Il2CppClass* elementClass, ::std::vector<Il2CppClass*>& interfaces);

    static void SetupArrayMethods(Il2CppClass* arrayClass)
    {
        size_t methodIndex = 0;
        uint8_t rank = arrayClass->rank;
        ::std::vector<Il2CppClass*> interfaces;
        CollectImplicitArrayInterfaces(arrayClass, interfaces);

        size_t methodCount = 3 + (rank > 1 ? 2 : 1) + interfaces.size() * GetArrayGenericMethodsCount();
        IL2CPP_ASSERT(methodCount <= std::numeric_limits<uint16_t>::max());
        arrayClass->method_count = static_cast<uint16_t>(methodCount);
        arrayClass->methods = (const MethodInfo**)MetadataCalloc(methodCount, sizeof(MethodInfo*));

        const Il2CppType** parameters = (const Il2CppType**)alloca(rank * sizeof(Il2CppType*));
        for (uint8_t i = 0; i < rank; i++)
            parameters[i] = &il2cpp_defaults.int32_class->byval_arg;
        arrayClass->methods[methodIndex++] = ConstructArrayMethod(arrayClass, ".ctor", &il2cpp_defaults.void_class->byval_arg, rank, parameters);

        if (rank > 1)
        {
            parameters = (const Il2CppType**)alloca(2 * rank * sizeof(Il2CppType*));
            for (uint8_t i = 0; i < 2 * rank; i++)
                parameters[i] = &il2cpp_defaults.int32_class->byval_arg;
            arrayClass->methods[methodIndex++] = ConstructArrayMethod(arrayClass, ".ctor", &il2cpp_defaults.void_class->byval_arg, 2 * rank, parameters);
        }

        parameters = (const Il2CppType**)alloca((rank + 1) * sizeof(Il2CppType*));
        for (uint8_t i = 0; i < rank; i++)
            parameters[i] = &il2cpp_defaults.int32_class->byval_arg;
        parameters[rank] = &arrayClass->element_class->byval_arg;
        arrayClass->methods[methodIndex++] = ConstructArrayMethod(arrayClass, "Set", &il2cpp_defaults.void_class->byval_arg, rank + 1, parameters);

        parameters = (const Il2CppType**)alloca(rank * sizeof(Il2CppType*));
        for (uint8_t i = 0; i < rank; i++)
            parameters[i] = &il2cpp_defaults.int32_class->byval_arg;
        arrayClass->methods[methodIndex++] = ConstructArrayMethod(arrayClass, "Address", &arrayClass->element_class->this_arg, rank, parameters);

        parameters = (const Il2CppType**)alloca(rank * sizeof(Il2CppType*));
        for (uint8_t i = 0; i < rank; i++)
            parameters[i] = &il2cpp_defaults.int32_class->byval_arg;
        arrayClass->methods[methodIndex++] = ConstructArrayMethod(arrayClass, "Get", &arrayClass->element_class->byval_arg, rank, parameters);

        IL2CPP_ASSERT(methodIndex <= std::numeric_limits<uint16_t>::max());
        PopulateArrayGenericMethods(arrayClass, static_cast<uint16_t>(methodIndex), s_GenericArrayMethods);
    }

    static void CollectImplicitArrayInterfacesFromElementClass(Il2CppClass* elementClass, ::std::vector<Il2CppClass*>& interfaces)
    {
        while (elementClass != NULL)
        {
            interfaces.push_back(elementClass);
            if (!elementClass->valuetype && elementClass != il2cpp_defaults.value_type_class && elementClass != il2cpp_defaults.enum_class)
            {
                void* iter = NULL;
                while (Il2CppClass* itf = Class::GetInterfaces(elementClass, &iter))
                    interfaces.push_back(itf);
            }

            if (elementClass->rank == 1)
            {
                ::std::vector<Il2CppClass*> elementInterfaces;
                CollectImplicitArrayInterfacesFromElementClass(elementClass->element_class, elementInterfaces);

                for (::std::vector<Il2CppClass*>::iterator iter = elementInterfaces.begin(); iter != elementInterfaces.end(); ++iter)
                {
                    Il2CppTypeVector genericArguments;
                    genericArguments.push_back(&(*iter)->byval_arg);

                    interfaces.push_back(Class::GetInflatedGenericInstanceClass(il2cpp_defaults.generic_ilist_class, genericArguments));
                    interfaces.push_back(Class::GetInflatedGenericInstanceClass(il2cpp_defaults.generic_icollection_class, genericArguments));
                    interfaces.push_back(Class::GetInflatedGenericInstanceClass(il2cpp_defaults.generic_ienumerable_class, genericArguments));

                    interfaces.push_back(Class::GetInflatedGenericInstanceClass(il2cpp_defaults.generic_ireadonlylist_class, genericArguments));
                    interfaces.push_back(Class::GetInflatedGenericInstanceClass(il2cpp_defaults.generic_ireadonlycollection_class, genericArguments));
                }
            }

            elementClass = Class::GetParent(elementClass);
            if (elementClass != NULL && (elementClass->valuetype || elementClass == il2cpp_defaults.value_type_class || elementClass == il2cpp_defaults.enum_class))
                break;
        }
    }

    static void CollectImplicitArrayInterfaces(Il2CppClass* arrayClass, ::std::vector<Il2CppClass*>& interfaces)
    {
        if (arrayClass->byval_arg.type != IL2CPP_TYPE_SZARRAY)
            return;

        CollectImplicitArrayInterfacesFromElementClass(arrayClass->element_class, interfaces);
    }

// note assuming list is ordered as IList, ICollection, IEnumerable

    static void CollectGenericArrayMethods(GenericArrayMethods& genericArrayMethods)
    {
        const size_t kNumGenericArrayMethods = 13;
        genericArrayMethods.reserve(kNumGenericArrayMethods);

        void* iter = NULL;
        while (const MethodInfo* method = Class::GetMethods(il2cpp_defaults.array_class, &iter))
        {
            if (strncmp(method->name, "InternalArray__", 15))
                continue;

            std::string name;
            std::string methodName;

            Il2CppClass* implementingInterface = NULL;
            if (!strncmp(method->name, "InternalArray__ICollection_", 27))
            {
                implementingInterface = il2cpp_defaults.generic_icollection_class;
                methodName = method->name + 27;
                name = StringUtils::Printf("System.Collections.Generic.ICollection`1.%s", method->name + 27);
            }
            else if (!strncmp(method->name, "InternalArray__IEnumerable_", 27))
            {
                implementingInterface = il2cpp_defaults.generic_ienumerable_class;
                methodName = method->name + 27;
                name = StringUtils::Printf("System.Collections.Generic.IEnumerable`1.%s", method->name + 27);
            }
            else if (!strncmp(method->name, "InternalArray__IReadOnlyList_", 29))
            {
                implementingInterface = il2cpp_defaults.generic_ireadonlylist_class;
                methodName = method->name + 29;
                name = StringUtils::Printf("System.Collections.Generic.IReadOnlyList`1.%s", method->name + 29);
            }
            else if (!strncmp(method->name, "InternalArray__IReadOnlyCollection_", 35))
            {
                implementingInterface = il2cpp_defaults.generic_ireadonlycollection_class;
                methodName = method->name + 35;
                name = StringUtils::Printf("System.Collections.Generic.IReadOnlyCollection`1.%s", method->name + 35);
            }
            else
            {
                implementingInterface = il2cpp_defaults.generic_ilist_class;
                methodName = method->name + 15;
                name = StringUtils::Printf("System.Collections.Generic.IList`1.%s", method->name + 15);
            }

            Class::Init(implementingInterface);

            const MethodInfo* matchingInterfacesMethod = NULL;
            for (int methodIndex = 0; methodIndex < implementingInterface->method_count; methodIndex++)
            {
                const MethodInfo* interfaceMethod = implementingInterface->methods[methodIndex];
                if (methodName == interfaceMethod->name)
                    matchingInterfacesMethod = interfaceMethod;
            }

            if (matchingInterfacesMethod != NULL)
            {
                GenericArrayMethod generiArrayMethod = { name, method, matchingInterfacesMethod };
                genericArrayMethods.push_back(generiArrayMethod);
            }
        }
    }

    static size_t GetArrayGenericMethodsCount()
    {
        if (s_GenericArrayMethods.size() == 0)
            CollectGenericArrayMethods(s_GenericArrayMethods);

        return s_GenericArrayMethods.size();
    }

    static MethodInfo* ConstructGenericArrayMethod(const GenericArrayMethod& genericArrayMethod, Il2CppClass* klass, Il2CppGenericContext* context)
    {
        MethodInfo* inflatedMethod = (MethodInfo*)MetadataCalloc(1, sizeof(MethodInfo));
        inflatedMethod->name = StringUtils::StringDuplicate(genericArrayMethod.name.c_str());
        inflatedMethod->klass = klass;

        const MethodInfo* methodToCopyDataFrom = genericArrayMethod.method;
        if (genericArrayMethod.method->is_generic)
        {
            const Il2CppGenericMethod* genericMethod = MetadataCache::GetGenericMethod(genericArrayMethod.method, context->class_inst, context->method_inst);
            methodToCopyDataFrom = GenericMethod::GetMethod(genericMethod);

            inflatedMethod->is_inflated = true;
            inflatedMethod->genericMethod = genericMethod;
            inflatedMethod->rgctx_data = methodToCopyDataFrom->rgctx_data;
        }

        inflatedMethod->slot = methodToCopyDataFrom->slot;
        inflatedMethod->parameters_count = methodToCopyDataFrom->parameters_count;
        inflatedMethod->parameters = methodToCopyDataFrom->parameters;
        inflatedMethod->return_type = methodToCopyDataFrom->return_type;

        inflatedMethod->methodPointer = methodToCopyDataFrom->methodPointer;
        inflatedMethod->invoker_method = methodToCopyDataFrom->invoker_method;

        return inflatedMethod;
    }

    static void PopulateArrayGenericMethods(Il2CppClass* klass, uint16_t offset, const GenericArrayMethods& genericArrayMethods)
    {
        for (int i = 0; i < klass->interface_offsets_count; i++)
        {
            Il2CppClass* interfaceType = klass->interfaceOffsets[i].interfaceType;
            if (!interfaceType->generic_class)
                continue;

            Il2CppClass* interfaceDefinition = GenericClass::GetTypeDefinition(interfaceType->generic_class);

            Il2CppGenericContext context = { 0 };
            Il2CppTypeVector types;
            types.push_back(interfaceType->generic_class->context.class_inst->type_argv[0]);
            context.method_inst = MetadataCache::GetGenericInst(types);

            for (GenericArrayMethods::const_iterator iter = genericArrayMethods.begin(); iter != genericArrayMethods.end(); ++iter)
            {
                if (iter->interfaceMethodDefinition->klass != interfaceDefinition)
                    continue;

                MethodInfo* arrayMethod = ConstructGenericArrayMethod(*iter, klass, &context);
                klass->methods[offset++] = arrayMethod;

                size_t vtableIndex = klass->interfaceOffsets[i].offset + iter->interfaceMethodDefinition->slot;
                klass->vtable[vtableIndex].method = arrayMethod;
                klass->vtable[vtableIndex].methodPtr = arrayMethod->methodPointer;
            }
        }
    }

    static void SetupArrayVTableAndInterfaceOffsets(Il2CppClass* klass)
    {
        Il2CppClass* arrayClass = Class::GetParent(klass);
        size_t arrayInterfacesCount = arrayClass->interface_offsets_count;

        ::std::vector<Il2CppClass*> interfaces;

        if (klass->byval_arg.type == IL2CPP_TYPE_SZARRAY)
        {
            CollectImplicitArrayInterfaces(klass, interfaces);
        }

        Il2CppRuntimeInterfaceOffsetPair* newInterfaceOffsets = (Il2CppRuntimeInterfaceOffsetPair*)MetadataMalloc((arrayInterfacesCount + kImplicitArrayInterfaceCount * interfaces.size()) * sizeof(Il2CppRuntimeInterfaceOffsetPair));
        memcpy(newInterfaceOffsets, arrayClass->interfaceOffsets, (arrayInterfacesCount) * sizeof(Il2CppRuntimeInterfaceOffsetPair));

        int32_t arrayVTableSlot = arrayClass->vtable_count;
        size_t slots = arrayVTableSlot + interfaces.size() * (il2cpp_defaults.generic_ilist_class->method_count + il2cpp_defaults.generic_icollection_class->method_count + il2cpp_defaults.generic_ienumerable_class->method_count);

        slots += interfaces.size() * (il2cpp_defaults.generic_ireadonlylist_class->method_count + il2cpp_defaults.generic_ireadonlycollection_class->method_count);

        memcpy(klass->vtable, arrayClass->vtable, arrayVTableSlot * sizeof(VirtualInvokeData));

        size_t index = arrayInterfacesCount;
        int32_t vtableSlot = arrayVTableSlot;
        for (::std::vector<Il2CppClass*>::iterator iter = interfaces.begin(); iter != interfaces.end(); iter++, index += kImplicitArrayInterfaceCount)
        {
            Il2CppTypeVector genericArguments;
            genericArguments.push_back(&(*iter)->byval_arg);

            newInterfaceOffsets[index].interfaceType = Class::GetInflatedGenericInstanceClass(il2cpp_defaults.generic_ilist_class, genericArguments);
            newInterfaceOffsets[index].offset = vtableSlot;
            vtableSlot += newInterfaceOffsets[index].interfaceType->method_count;

            newInterfaceOffsets[index + 1].interfaceType = Class::GetInflatedGenericInstanceClass(il2cpp_defaults.generic_icollection_class, genericArguments);
            newInterfaceOffsets[index + 1].offset = vtableSlot;
            vtableSlot += newInterfaceOffsets[index + 1].interfaceType->method_count;

            newInterfaceOffsets[index + 2].interfaceType = Class::GetInflatedGenericInstanceClass(il2cpp_defaults.generic_ienumerable_class, genericArguments);
            newInterfaceOffsets[index + 2].offset = vtableSlot;
            vtableSlot += newInterfaceOffsets[index + 2].interfaceType->method_count;

            newInterfaceOffsets[index + 3].interfaceType = Class::GetInflatedGenericInstanceClass(il2cpp_defaults.generic_ireadonlylist_class, genericArguments);
            newInterfaceOffsets[index + 3].offset = vtableSlot;
            vtableSlot += newInterfaceOffsets[index + 3].interfaceType->method_count;

            newInterfaceOffsets[index + 4].interfaceType = Class::GetInflatedGenericInstanceClass(il2cpp_defaults.generic_ireadonlycollection_class, genericArguments);
            newInterfaceOffsets[index + 4].offset = vtableSlot;
            vtableSlot += newInterfaceOffsets[index + 4].interfaceType->method_count;
        }

        size_t interfaceOffsetsCount = arrayInterfacesCount + kImplicitArrayInterfaceCount * interfaces.size();
        IL2CPP_ASSERT(interfaceOffsetsCount <= std::numeric_limits<uint16_t>::max());
        klass->interface_offsets_count = static_cast<uint16_t>(interfaceOffsetsCount);
        klass->interfaceOffsets = newInterfaceOffsets;
    }

    void SetupCastClass(Il2CppClass *arrayType)
    {
        Il2CppClass *elementType = arrayType->element_class;

        if (elementType->enumtype)
            arrayType->castClass = elementType->element_class;
        else
            arrayType->castClass = elementType;

        if (arrayType->castClass == il2cpp_defaults.sbyte_class)
            arrayType->castClass = il2cpp_defaults.byte_class;
        else if (arrayType->castClass == il2cpp_defaults.uint16_class)
            arrayType->castClass = il2cpp_defaults.int16_class;
        else if (arrayType->castClass == il2cpp_defaults.uint32_class)
            arrayType->castClass = il2cpp_defaults.int32_class;
        else if (arrayType->castClass == il2cpp_defaults.uint64_class)
            arrayType->castClass = il2cpp_defaults.int64_class;
#if IL2CPP_SIZEOF_VOID_P == 8
        else if (arrayType->castClass == il2cpp_defaults.int_class ||
                 arrayType->castClass == il2cpp_defaults.uint_class)
            arrayType->castClass = il2cpp_defaults.int64_class;
#else
        else if (arrayType->castClass == il2cpp_defaults.int_class ||
                 arrayType->castClass == il2cpp_defaults.uint_class)
            arrayType->castClass = il2cpp_defaults.int32_class;
#endif

        arrayType->has_references = Type::IsReference(&elementType->byval_arg) || elementType->has_references;
    }

    void ArrayMetadata::SetupArrayInterfaces(Il2CppClass* klass, const FastAutoLock& lock)
    {
        if (klass->byval_arg.type == IL2CPP_TYPE_SZARRAY)
        {
            IL2CPP_ASSERT(klass->implementedInterfaces == NULL);

            Il2CppTypeVector genericArguments;
            genericArguments.push_back(&klass->element_class->byval_arg);

            IL2CPP_ASSERT(klass->interfaces_count == kImplicitArrayInterfaceCount);
            klass->implementedInterfaces = (Il2CppClass**)MetadataMalloc(klass->interfaces_count * sizeof(Il2CppClass*));
            klass->implementedInterfaces[0] = Class::GetInflatedGenericInstanceClass(il2cpp_defaults.generic_ilist_class, genericArguments);
            IL2CPP_ASSERT(klass->implementedInterfaces[0]);
            klass->implementedInterfaces[1] = Class::GetInflatedGenericInstanceClass(il2cpp_defaults.generic_icollection_class, genericArguments);
            IL2CPP_ASSERT(klass->implementedInterfaces[1]);
            klass->implementedInterfaces[2] = Class::GetInflatedGenericInstanceClass(il2cpp_defaults.generic_ienumerable_class, genericArguments);
            IL2CPP_ASSERT(klass->implementedInterfaces[2]);
            klass->implementedInterfaces[3] = Class::GetInflatedGenericInstanceClass(il2cpp_defaults.generic_ireadonlylist_class, genericArguments);
            IL2CPP_ASSERT(klass->implementedInterfaces[3]);
            klass->implementedInterfaces[4] = Class::GetInflatedGenericInstanceClass(il2cpp_defaults.generic_ireadonlycollection_class, genericArguments);
            IL2CPP_ASSERT(klass->implementedInterfaces[4]);
        }
    }

    void ArrayMetadata::SetupArrayVTable(Il2CppClass* klass, const FastAutoLock& lock)
    {
        // we assume we are being called as part of Class::Init and that the element class has already been initialized
        IL2CPP_ASSERT(klass->element_class->initialized);

        SetupCastClass(klass);
        SetupArrayVTableAndInterfaceOffsets(klass);
        SetupArrayMethods(klass);
    }

    struct SZArrayClassHash
    {
        size_t operator()(const Il2CppClass* arrayClass) const
        {
            return Il2CppTypeHash::Hash(&arrayClass->byval_arg);
        }
    };

    struct SZArrayClassCompare
    {
        bool operator()(const Il2CppClass* arrayClass1, const Il2CppClass* arrayClass2) const
        {
            return Il2CppTypeEqualityComparer::AreEqual(&arrayClass1->byval_arg, &arrayClass2->byval_arg);
        }
    };

    struct ArrayClassHash
    {
        size_t operator()(const std::pair<Il2CppClass*, uint32_t>& arrayClass) const
        {
            return Il2CppTypeHash::Hash(&arrayClass.first->byval_arg) * arrayClass.second;
        }
    };

    struct ArrayClassCompare
    {
        bool operator()(const std::pair<Il2CppClass*, uint32_t>& arrayClass1, const std::pair<Il2CppClass*, uint32_t>& arrayClass2) const
        {
            return Il2CppTypeEqualityComparer::AreEqual(&arrayClass1.first->byval_arg, &arrayClass2.first->byval_arg) && arrayClass1.second == arrayClass2.second;
        }
    };

    typedef Il2CppHashMap<Il2CppClass*, Il2CppClass*, SZArrayClassHash, SZArrayClassCompare> SZArrayClassMap;
    typedef Il2CppHashMap<std::pair<Il2CppClass*, uint32_t>, Il2CppClass*, ArrayClassHash, ArrayClassCompare> ArrayClassMap;

    SZArrayClassMap s_SZArrayClassMap;
    ArrayClassMap s_ArrayClassMap;

    void ArrayMetadata::Clear()
    {
        s_SZArrayClassMap.clear();
        s_ArrayClassMap.clear();
    }

    Il2CppClass* ArrayMetadata::GetBoundedArrayClass(Il2CppClass* elementClass, uint32_t rank, bool bounded)
    {
        FastAutoLock lock(&il2cpp::vm::g_MetadataLock);
        IL2CPP_NOT_IMPLEMENTED_NO_ASSERT(ArrayMetadata::GetBoundedArrayClass, "Use more granular lock for looking up arrays, but then handle race between lookup, construction, and caching");

        IL2CPP_ASSERT(rank <= 255);

        if (rank > 1)
            bounded = false;

        if (rank > 1 || bounded)
        {
            ArrayClassMap::const_iterator iter = s_ArrayClassMap.find(std::make_pair(elementClass, rank));
            if (iter != s_ArrayClassMap.end())
                return iter->second;
        }
        else
        {
            SZArrayClassMap::const_iterator iter = s_SZArrayClassMap.find(elementClass);
            if (iter != s_SZArrayClassMap.end())
                return iter->second;
        }

        Il2CppClass* arrayClass = il2cpp_defaults.array_class;
        Class::Init(arrayClass);

        //count number of virtual call slots for array class
        ::std::vector<Il2CppClass*> interfaces;

        if (rank <= 1 && !bounded)
            CollectImplicitArrayInterfacesFromElementClass(elementClass, interfaces);

        size_t slots = arrayClass->vtable_count + interfaces.size() * (il2cpp_defaults.generic_ilist_class->method_count + il2cpp_defaults.generic_icollection_class->method_count + il2cpp_defaults.generic_ienumerable_class->method_count);

        slots += interfaces.size() * (il2cpp_defaults.generic_ireadonlylist_class->method_count + il2cpp_defaults.generic_ireadonlycollection_class->method_count);

        Il2CppClass* klass = (Il2CppClass*)MetadataCalloc(1, sizeof(Il2CppClass) + (slots * sizeof(VirtualInvokeData)));
        klass->klass = klass;
        klass->image = elementClass->image;
        // can share the const char* since it's immutable
        klass->namespaze = elementClass->namespaze;
        klass->name = GetArrayName(elementClass->name, rank, bounded);

        klass->parent = il2cpp_defaults.array_class;

        klass->flags = TYPE_ATTRIBUTE_AUTO_LAYOUT | TYPE_ATTRIBUTE_ANSI_CLASS | TYPE_ATTRIBUTE_CLASS | TYPE_ATTRIBUTE_PUBLIC | TYPE_ATTRIBUTE_SEALED | TYPE_ATTRIBUTE_SERIALIZABLE;
        klass->rank = rank;

        klass->instance_size = Class::GetInstanceSize(arrayClass);
        klass->vtable_count = static_cast<uint16_t>(slots);

        // need this before we access the size or has_references
        Class::SetupFields(elementClass);

        klass->element_size = Class::GetArrayElementSize(elementClass);
        klass->native_size = klass->thread_static_fields_offset = -1;

        klass->has_references = Type::IsReference(&elementClass->byval_arg) || elementClass->has_references;

        klass->size_inited = true; // set only after instance_size and has_references are set

        klass->element_class = elementClass;

        if (rank > 1 || bounded)
        {
            Il2CppArrayType *at = (Il2CppArrayType*)MetadataCalloc(1, sizeof(Il2CppArrayType));
            klass->byval_arg.type = IL2CPP_TYPE_ARRAY;
            klass->byval_arg.data.array = at;
            at->etype = &elementClass->byval_arg;
            at->rank = rank;
        }
        else
        {
            klass->byval_arg.type = IL2CPP_TYPE_SZARRAY;
            klass->byval_arg.data.type = &elementClass->byval_arg;
        }

        klass->this_arg = klass->byval_arg;
        klass->this_arg.byref = 1;

        if (rank > 1 || bounded)
        {
            klass->interfaces_count = 0;
        }
        else
        {
            klass->interfaces_count = kImplicitArrayInterfaceCount;
        }

        klass->interopData = MetadataCache::GetInteropDataForType(&klass->byval_arg);

        if (rank > 1 || bounded)
            s_ArrayClassMap.insert(std::make_pair(ArrayClassMap::key_type(std::make_pair(klass->element_class, klass->rank)), klass));
        else
            s_SZArrayClassMap.insert(std::make_pair(klass->element_class, klass));

        return klass;
    }

    void ArrayMetadata::WalkSZArrays(ArrayTypeWalkCallback callback, void* context)
    {
        FastAutoLock lock(&il2cpp::vm::g_MetadataLock);

        for (SZArrayClassMap::iterator it = s_SZArrayClassMap.begin(); it != s_SZArrayClassMap.end(); it++)
        {
            callback(it->second, context);
        }
    }

    void ArrayMetadata::WalkArrays(ArrayTypeWalkCallback callback, void* context)
    {
        FastAutoLock lock(&il2cpp::vm::g_MetadataLock);

        for (ArrayClassMap::iterator it = s_ArrayClassMap.begin(); it != s_ArrayClassMap.end(); it++)
        {
            callback(it->second, context);
        }
    }
} /* namespace vm */
} /* namespace il2cpp */
