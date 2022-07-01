#include "il2cpp-config.h"
#include "metadata/GenericMetadata.h"
#include "os/LibraryLoader.h"
#include "os/WindowsRuntime.h"
#include "utils/Il2CppHStringReference.h"
#include "utils/StringUtils.h"
#include "utils/StringViewUtils.h"
#include "vm/AssemblyName.h"
#include "vm/Class.h"
#include "vm/Exception.h"
#include "vm/GenericClass.h"
#include "vm/Image.h"
#include "vm/MetadataCache.h"
#include "vm/Type.h"
#include "vm/WindowsRuntime.h"

namespace il2cpp
{
namespace vm
{
    const char kArrayTypePrefixUtf8[] = "Windows.Foundation.IReferenceArray`1<";
    const Il2CppNativeChar kArrayTypePrefix[] = IL2CPP_NATIVE_STRING("Windows.Foundation.IReferenceArray`1<");
    const Il2CppNativeChar kIReferencePrefix[] = IL2CPP_NATIVE_STRING("Windows.Foundation.IReference`1<");
    const Il2CppNativeChar kArrayTypeOrIReferencePrefix[] = IL2CPP_NATIVE_STRING("Windows.Foundation.IReference");
    const Il2CppNativeChar kArrayTypePostprefix[] = IL2CPP_NATIVE_STRING("Array`1<");
    const Il2CppNativeChar kIReferencePostprefix[] = IL2CPP_NATIVE_STRING("`1<");

    Il2CppIActivationFactory* WindowsRuntime::GetActivationFactory(const utils::StringView<Il2CppNativeChar>& runtimeClassName)
    {
        utils::Il2CppHStringReference className(runtimeClassName);
        Il2CppIActivationFactory* factory = NULL;
        il2cpp_hresult_t hr = os::WindowsRuntime::GetActivationFactory(className, &factory);

        if (IL2CPP_HR_SUCCEEDED(hr))
            return factory;

        if (hr != IL2CPP_REGDB_E_CLASSNOTREG)
            Exception::Raise(hr, false);

        // If GetActivationFactory doesn't find the class, we can still try to find it manually
        // All Windows runtime classes must be in namespaces, and that class has to be in a DLL
        // that is named after the namespace of a part of it.
        // For example, MyNamespace.MySubNamespace.MyClass can be in either
        // MyNamespace.MySubNamespace.dll or MyNamespace.dll
        IL2CPP_ASSERT(runtimeClassName.Length() > 1);
        size_t namespaceEnd = runtimeClassName.Length() - 1;

        do
        {
            namespaceEnd--;
        }
        while (namespaceEnd > 0 && runtimeClassName[namespaceEnd] != '.');

        Il2CppNativeChar* nativeDll = static_cast<Il2CppNativeChar*>(alloca((namespaceEnd + 5) * sizeof(Il2CppNativeChar)));
        memcpy(nativeDll, runtimeClassName.Str(), namespaceEnd * sizeof(Il2CppNativeChar));

        while (namespaceEnd > 0)
        {
            memcpy(nativeDll + namespaceEnd, IL2CPP_NATIVE_STRING(".dll"), 4 * sizeof(Il2CppNativeChar));
            nativeDll[namespaceEnd + 4] = 0;

            void* dynamicLibrary = os::LibraryLoader::LoadDynamicLibrary(utils::StringView<Il2CppNativeChar>(nativeDll, namespaceEnd + 4));
            if (dynamicLibrary != NULL)
            {
                typedef il2cpp_hresult_t(STDCALL * DllGetActivationFactory)(Il2CppHString activatableClassId, Il2CppIActivationFactory** factory);
                DllGetActivationFactory dllGetActivationFactory = reinterpret_cast<DllGetActivationFactory>(os::LibraryLoader::GetFunctionPointer(dynamicLibrary, "DllGetActivationFactory"));

                if (dllGetActivationFactory != NULL)
                {
                    hr = dllGetActivationFactory(className, &factory);

                    if (IL2CPP_HR_SUCCEEDED(hr))
                        return factory;

                    if (hr != IL2CPP_REGDB_E_CLASSNOTREG)
                        Exception::Raise(hr, false);
                }
            }

            do
            {
                namespaceEnd--;
            }
            while (namespaceEnd > 0 && runtimeClassName[namespaceEnd] != '.');
        }

        Exception::Raise(IL2CPP_REGDB_E_CLASSNOTREG, false);

        return NULL;
    }

    static bool IsWindowsRuntimePrimitiveType(const Il2CppType* type, Il2CppClass*& outCachedNonPrimitiveClass)
    {
        if (type == NULL)
            return false;

        switch (type->type)
        {
            case IL2CPP_TYPE_BOOLEAN:
            case IL2CPP_TYPE_CHAR:
            case IL2CPP_TYPE_U1:
            case IL2CPP_TYPE_I2:
            case IL2CPP_TYPE_U2:
            case IL2CPP_TYPE_I4:
            case IL2CPP_TYPE_U4:
            case IL2CPP_TYPE_I8:
            case IL2CPP_TYPE_U8:
            case IL2CPP_TYPE_R4:
            case IL2CPP_TYPE_R8:
            case IL2CPP_TYPE_OBJECT:
            case IL2CPP_TYPE_STRING:
                return true;

            default:
                break;
        }

        Il2CppClass* klass = Class::FromIl2CppType(type);
        if (klass == il2cpp_defaults.system_guid_class)
            return true;

        outCachedNonPrimitiveClass = klass;
        return false;
    }

    static Il2CppWindowsRuntimeTypeKind GetWindowsRuntimeTypeKind(const Il2CppType* type, Il2CppClass*& outCachedNonPrimitiveClass)
    {
        if (type == NULL)
            return kTypeKindCustom;

        switch (type->type)
        {
            case IL2CPP_TYPE_BOOLEAN:
            case IL2CPP_TYPE_CHAR:
            case IL2CPP_TYPE_U1:
            case IL2CPP_TYPE_I2:
            case IL2CPP_TYPE_U2:
            case IL2CPP_TYPE_I4:
            case IL2CPP_TYPE_U4:
            case IL2CPP_TYPE_I8:
            case IL2CPP_TYPE_U8:
            case IL2CPP_TYPE_R4:
            case IL2CPP_TYPE_R8:
            case IL2CPP_TYPE_OBJECT:
            case IL2CPP_TYPE_STRING:
                return kTypeKindPrimitive;

            default:
                break;
        }

        Il2CppClass* klass = Class::FromIl2CppType(type);
        if (klass == il2cpp_defaults.system_guid_class)
            return kTypeKindPrimitive;

        outCachedNonPrimitiveClass = klass;
        if (klass->rank > 0)
        {
            Il2CppClass* cachedElementClass;
            if (GetWindowsRuntimeTypeKind(&klass->element_class->byval_arg, cachedElementClass) != kTypeKindCustom)
                return kTypeKindMetadata;
        }
        else
        {
            const char* windowsRuntimeTypeName = MetadataCache::GetWindowsRuntimeClassName(klass);
            if (windowsRuntimeTypeName != NULL)
                return kTypeKindMetadata;

            if (strcmp(klass->image->name, "WindowsRuntimeMetadata") == 0)
            {
                Il2CppGenericClass* genericClass = klass->generic_class;
                if (genericClass == NULL)
                    return kTypeKindMetadata;

                const Il2CppGenericInst* classInst = genericClass->context.class_inst;
                IL2CPP_ASSERT(classInst != NULL);

                uint32_t genericArgumentCount = classInst->type_argc;
                for (uint32_t i = 0; i < genericArgumentCount; i++)
                {
                    Il2CppClass* cachedGenericArgumentClass;
                    if (GetWindowsRuntimeTypeKind(classInst->type_argv[i], cachedGenericArgumentClass) == kTypeKindCustom)
                        return kTypeKindCustom;
                }

                return kTypeKindMetadata;
            }
        }

        return kTypeKindCustom;
    }

    static utils::StringView<Il2CppNativeChar> GetWindowsRuntimePrimitiveTypeName(const Il2CppType* type)
    {
        switch (type->type)
        {
            case IL2CPP_TYPE_BOOLEAN:
                return IL2CPP_NATIVE_STRING("Boolean");

            case IL2CPP_TYPE_CHAR:
                return IL2CPP_NATIVE_STRING("Char16");

            case IL2CPP_TYPE_U1:
                return IL2CPP_NATIVE_STRING("UInt8");

            case IL2CPP_TYPE_I2:
                return IL2CPP_NATIVE_STRING("Int16");

            case IL2CPP_TYPE_U2:
                return IL2CPP_NATIVE_STRING("UInt16");

            case IL2CPP_TYPE_I4:
                return IL2CPP_NATIVE_STRING("Int32");

            case IL2CPP_TYPE_U4:
                return IL2CPP_NATIVE_STRING("UInt32");

            case IL2CPP_TYPE_I8:
                return IL2CPP_NATIVE_STRING("Int64");

            case IL2CPP_TYPE_U8:
                return IL2CPP_NATIVE_STRING("UInt64");

            case IL2CPP_TYPE_R4:
                return IL2CPP_NATIVE_STRING("Single");

            case IL2CPP_TYPE_R8:
                return IL2CPP_NATIVE_STRING("Double");

            case IL2CPP_TYPE_OBJECT:
                return IL2CPP_NATIVE_STRING("Object");

            case IL2CPP_TYPE_STRING:
                return IL2CPP_NATIVE_STRING("String");

            case IL2CPP_TYPE_VALUETYPE:
                return IL2CPP_NATIVE_STRING("Guid");

            default:
                IL2CPP_UNREACHABLE;
                return IL2CPP_NATIVE_STRING("");
        }
    }

    // This is code duplication... but there isn't a better name to achieve good performance for both primitive and metadata types otherwise
    static utils::StringView<char> GetWindowsRuntimePrimitiveTypeNameUtf8(const Il2CppType* type)
    {
        switch (type->type)
        {
            case IL2CPP_TYPE_BOOLEAN:
                return "Boolean";

            case IL2CPP_TYPE_CHAR:
                return "Char16";

            case IL2CPP_TYPE_U1:
                return "UInt8";

            case IL2CPP_TYPE_I2:
                return "Int16";

            case IL2CPP_TYPE_U2:
                return "UInt16";

            case IL2CPP_TYPE_I4:
                return "Int32";

            case IL2CPP_TYPE_U4:
                return "UInt32";

            case IL2CPP_TYPE_I8:
                return "Int64";

            case IL2CPP_TYPE_U8:
                return "UInt64";

            case IL2CPP_TYPE_R4:
                return "Single";

            case IL2CPP_TYPE_R8:
                return "Double";

            case IL2CPP_TYPE_OBJECT:
                return "Object";

            case IL2CPP_TYPE_STRING:
                return "String";

            case IL2CPP_TYPE_VALUETYPE:
                return "Guid";

            default:
                IL2CPP_UNREACHABLE;
                return "";
        }
    }

    static std::string GetWindowsRuntimeTypeNameFromWinmdReference(Il2CppClass* klass);

    static std::string GetWindowsRuntimeMetadataTypeNameUtf8(Il2CppClass* klass)
    {
        if (klass->rank > 0)
        {
            std::string typeName, elementMetadataTypeName;
            const Il2CppType* elementType = &klass->element_class->byval_arg;
            Il2CppClass* elementClass = NULL;
            bool elementIsPrimitive = IsWindowsRuntimePrimitiveType(elementType, elementClass);
            utils::StringView<char> elementTypeName(utils::StringView<char>::Empty());

            // Optimization: time spent in GetWindowsRuntimeMetadataTypeName is dominated by string allocations,
            // so try to reserve needed space on a string in advance.
            if (elementIsPrimitive)
            {
                elementTypeName = GetWindowsRuntimePrimitiveTypeNameUtf8(elementType);
            }
            else
            {
                elementMetadataTypeName = GetWindowsRuntimeMetadataTypeNameUtf8(elementClass);
                elementTypeName = STRING_TO_STRINGVIEW(elementMetadataTypeName);
            }

            size_t spaceRequired = IL2CPP_ARRAY_SIZE(kArrayTypePrefixUtf8) + elementTypeName.Length() + 1 /* '>' */ - 1 /* minus null terminator from IL2CPP_ARRAY_SIZE */;
            typeName.reserve(spaceRequired);

            typeName.append(kArrayTypePrefixUtf8);
            typeName.append(elementTypeName.Str(), elementTypeName.Length());
            typeName.push_back('>');

            return typeName;
        }

        const char* windowsRuntimeTypeName = MetadataCache::GetWindowsRuntimeClassName(klass);
        if (windowsRuntimeTypeName != NULL)
            return windowsRuntimeTypeName;

        return GetWindowsRuntimeTypeNameFromWinmdReference(klass);
    }

    static std::string GetWindowsRuntimeTypeNameFromWinmdReference(Il2CppClass* klass)
    {
        IL2CPP_ASSERT(strcmp(klass->image->name, "WindowsRuntimeMetadata") == 0 && "Windows Runtime type kind was Metadata but it did not come from a Windows Runtime component.");

        std::string typeName;
        size_t namespaceLength = strlen(klass->namespaze);
        size_t nameLength = strlen(klass->name);

        typeName.reserve(namespaceLength + 1 + nameLength);

        typeName.append(klass->namespaze, namespaceLength);
        typeName.push_back('.');
        typeName.append(klass->name, nameLength);

        Il2CppGenericClass* genericClass = klass->generic_class;
        if (genericClass == NULL)
            return typeName;

        const Il2CppGenericInst* classInst = genericClass->context.class_inst;
        IL2CPP_ASSERT(classInst != NULL);

        typeName += '<';

        uint32_t genericArgumentCount = classInst->type_argc;
        for (uint32_t i = 0; i < genericArgumentCount; i++)
        {
            if (i != 0)
                typeName += ',';

            const Il2CppType* genericArgumentType = classInst->type_argv[i];
            Il2CppClass* genericArgumentClass = NULL;

            if (IsWindowsRuntimePrimitiveType(genericArgumentType, genericArgumentClass))
            {
                utils::StringView<char> primitiveTypeName = GetWindowsRuntimePrimitiveTypeNameUtf8(genericArgumentType);
                typeName.append(primitiveTypeName.Str(), primitiveTypeName.Length());
            }
            else
            {
                // Windows Runtime metadata types can't have generic arguments of Custom type, thus the argument is metadata type too
                typeName += GetWindowsRuntimeMetadataTypeNameUtf8(genericArgumentClass);
            }
        }

        typeName += '>';
        return typeName;
    }

    static Il2CppHString GetWindowsRuntimeMetadataTypeName(Il2CppClass* klass)
    {
        // Optimization: for type arrays we can construct native string in place and avoid extra UTF8 -> UTF16 conversion
        // This makes type name retrieval 4 times faster!
        if (klass->rank > 0)
        {
            const Il2CppType* elementType = &klass->element_class->byval_arg;
            Il2CppClass* elementClass = NULL;

            utils::StringView<Il2CppNativeChar> elementTypeName(utils::StringView<Il2CppNativeChar>::Empty());
            Il2CppHString elementMetadataTypeName = NULL;

            bool isElementTypePrimitive = IsWindowsRuntimePrimitiveType(elementType, elementClass);
            if (isElementTypePrimitive)
            {
                elementTypeName = GetWindowsRuntimePrimitiveTypeName(elementType);
            }
            else
            {
                elementMetadataTypeName = GetWindowsRuntimeMetadataTypeName(elementClass);

                uint32_t elementTypeNameLength;
                const Il2CppNativeChar* elementMetadataTypeNamePtr = os::WindowsRuntime::GetNativeHStringBuffer(elementMetadataTypeName, &elementTypeNameLength);
                elementTypeName = utils::StringView<Il2CppNativeChar>(elementMetadataTypeNamePtr, elementTypeNameLength);
            }

            size_t offsetInChars = 0;
            size_t spaceRequired = IL2CPP_ARRAY_SIZE(kArrayTypePrefix) + elementTypeName.Length() + 1 /* '>' */ - 1 /* minus null terminator from IL2CPP_ARRAY_SIZE */;

            Il2CppNativeChar* buffer;
            void* hstringBufferHandle = WindowsRuntime::PreallocateHStringBuffer(static_cast<uint32_t>(spaceRequired), &buffer);

            memcpy(buffer, kArrayTypePrefix, sizeof(kArrayTypePrefix) - sizeof(Il2CppNativeChar));
            offsetInChars += IL2CPP_ARRAY_SIZE(kArrayTypePrefix) - 1;

            memcpy(buffer + offsetInChars, elementTypeName.Str(), elementTypeName.Length() * sizeof(Il2CppNativeChar));
            offsetInChars += elementTypeName.Length();

            buffer[offsetInChars] = static_cast<Il2CppNativeChar>('>');

            if (!isElementTypePrimitive)
                WindowsRuntime::DeleteHString(elementMetadataTypeName);

            return WindowsRuntime::PromoteHStringBuffer(hstringBufferHandle);
        }

        // Note: don't put 'windowsRuntimeTypeName' into an std::string to save an allocation
        const char* windowsRuntimeTypeName = MetadataCache::GetWindowsRuntimeClassName(klass);
        if (windowsRuntimeTypeName != NULL)
        {
            Il2CppNativeString typeName = utils::StringUtils::Utf8ToNativeString(windowsRuntimeTypeName);
            return WindowsRuntime::CreateHString(STRING_TO_STRINGVIEW(typeName));
        }

        std::string typeNameUtf8 = GetWindowsRuntimeTypeNameFromWinmdReference(klass);
        Il2CppNativeString typeName = utils::StringUtils::Utf8ToNativeString(typeNameUtf8);
        return WindowsRuntime::CreateHString(STRING_TO_STRINGVIEW(typeName));
    }

    static Il2CppHString GetWindowsRuntimeCustomTypeName(const Il2CppType* type)
    {
        std::string typeNameUtf8 = Type::GetName(type, IL2CPP_TYPE_NAME_FORMAT_ASSEMBLY_QUALIFIED);
        Il2CppNativeString typeName = utils::StringUtils::Utf8ToNativeString(typeNameUtf8);
        return WindowsRuntime::CreateHString(STRING_TO_STRINGVIEW(typeName));
    }

    void WindowsRuntime::MarshalTypeToNative(const Il2CppType* type, Il2CppWindowsRuntimeTypeName& nativeType)
    {
        if (type == NULL)
        {
            nativeType.typeKind = kTypeKindCustom;
            nativeType.typeName = NULL;
            return;
        }

        Il2CppClass* cachedClass = NULL;
        nativeType.typeKind = GetWindowsRuntimeTypeKind(type, cachedClass);

        switch (nativeType.typeKind)
        {
            case kTypeKindPrimitive:
                nativeType.typeName = CreateHString(GetWindowsRuntimePrimitiveTypeName(type));
                break;

            case kTypeKindMetadata:
                nativeType.typeName = GetWindowsRuntimeMetadataTypeName(cachedClass);
                break;

            case kTypeKindCustom:
                nativeType.typeName = GetWindowsRuntimeCustomTypeName(type);
                break;

            default:
                IL2CPP_UNREACHABLE;
        }
    }

    static REAL_NORETURN IL2CPP_NO_INLINE void ThrowUnexpectedTypeKindException()
    {
        const char kMessage[] = "Unexpected TypeKind when marshaling Windows.Foundation.TypeName. ";
        Il2CppException* exception = Exception::GetArgumentException("", kMessage);
        Exception::Raise(exception);
        IL2CPP_UNREACHABLE;
    }

    static REAL_NORETURN IL2CPP_NO_INLINE void ThrowWindowsRuntimeTypeNotFoundException(utils::StringView<Il2CppNativeChar> typeName)
    {
        std::string typeNameUtf8 = utils::StringUtils::NativeStringToUtf8(typeName.Str(), static_cast<uint32_t>(typeName.Length()));
        Il2CppException* typeLoadException = Exception::GetTypeLoadExceptionForWindowsRuntimeType(utils::StringView<char>::Empty(), STRING_TO_STRINGVIEW(typeNameUtf8));
        Exception::Raise(typeLoadException);
        IL2CPP_UNREACHABLE;
    }

    static Il2CppClass* GetClassFromPrimitiveTypeName(utils::StringView<Il2CppNativeChar> typeName, bool throwOnFailure);
    static Il2CppClass* GetClassFromMetadataTypeName(utils::StringView<Il2CppNativeChar> typeName, bool throwOnFailure);

    static inline Il2CppClass* GetClassFromPrimitiveOrMetadataTypeName(utils::StringView<Il2CppNativeChar> typeName, bool throwOnFailure)
    {
        // Try finding type as primitive type first
        // If that fails, try finding it as metadata type
        Il2CppClass* klass = GetClassFromPrimitiveTypeName(typeName, throwOnFailure);
        if (klass != NULL)
            return klass;

        return GetClassFromMetadataTypeName(typeName, throwOnFailure);
    }

    static Il2CppClass* GetClassFromPrimitiveTypeName(utils::StringView<Il2CppNativeChar> typeName, bool throwOnFailure)
    {
        uint32_t characterSum = 0;
        for (uint32_t i = 0; i < typeName.Length(); i++)
            characterSum += typeName[i];

        // Nothing like an (almost) perfect hash function. Thanks for the idea, @andrei!
        switch (characterSum)
        {
            case 393:
                if (utils::StringUtils::Equals(typeName, IL2CPP_NATIVE_STRING("Guid")))
                    return il2cpp_defaults.system_guid_class;

                break;

            case 400:
                if (utils::StringUtils::Equals(typeName, IL2CPP_NATIVE_STRING("Int32")))
                    return il2cpp_defaults.int32_class;

                break;

            case 402:
                if (utils::StringUtils::Equals(typeName, IL2CPP_NATIVE_STRING("Int16")))
                    return il2cpp_defaults.int16_class;

                break;

            case 405:
                if (utils::StringUtils::Equals(typeName, IL2CPP_NATIVE_STRING("Int64")))
                    return il2cpp_defaults.int64_class;

                break;

            case 440:
                if (utils::StringUtils::Equals(typeName, IL2CPP_NATIVE_STRING("UInt8")))
                    return il2cpp_defaults.byte_class;

                break;

            case 485:
                if (utils::StringUtils::Equals(typeName, IL2CPP_NATIVE_STRING("Char16")))
                    return il2cpp_defaults.char_class;

                if (utils::StringUtils::Equals(typeName, IL2CPP_NATIVE_STRING("UInt32")))
                    return il2cpp_defaults.uint32_class;

                break;

            case 487:
                if (utils::StringUtils::Equals(typeName, IL2CPP_NATIVE_STRING("UInt16")))
                    return il2cpp_defaults.uint16_class;

                break;

            case 490:
                if (utils::StringUtils::Equals(typeName, IL2CPP_NATIVE_STRING("UInt64")))
                    return il2cpp_defaults.uint64_class;

                break;

            case 599:
                if (utils::StringUtils::Equals(typeName, IL2CPP_NATIVE_STRING("Object")))
                    return il2cpp_defaults.object_class;

                break;

            case 603:
                if (utils::StringUtils::Equals(typeName, IL2CPP_NATIVE_STRING("Double")))
                    return il2cpp_defaults.double_class;

                break;

            case 610:
                if (utils::StringUtils::Equals(typeName, IL2CPP_NATIVE_STRING("Single")))
                    return il2cpp_defaults.single_class;

                break;

            case 631:
                if (utils::StringUtils::Equals(typeName, IL2CPP_NATIVE_STRING("String")))
                    return il2cpp_defaults.string_class;

                break;

            case 704:
                if (utils::StringUtils::Equals(typeName, IL2CPP_NATIVE_STRING("Boolean")))
                    return il2cpp_defaults.boolean_class;

                break;
        }

        if (throwOnFailure)
        {
            // Is this actually a metadata type? If so, throw unexpected type kind exception
            if (GetClassFromMetadataTypeName(typeName, false) != NULL)
                ThrowUnexpectedTypeKindException();

            ThrowWindowsRuntimeTypeNotFoundException(typeName);
        }

        return NULL;
    }

    static Il2CppClass* GetGenericInstanceClassFromMetadataTypeName(utils::StringView<Il2CppNativeChar> typeName)
    {
        IL2CPP_ASSERT(typeName[typeName.Length() - 1] == '>');

        size_t backtickIndex = typeName.Find('`');
        if (backtickIndex == utils::StringView<Il2CppNativeChar>::NPos())
            return NULL;

        size_t genericArgumentStartIndex = typeName.Find('<', backtickIndex + 1);
        if (genericArgumentStartIndex == utils::StringView<Il2CppNativeChar>::NPos())
            return NULL;

        int genericArgumentCount;
        utils::StringView<Il2CppNativeChar> genericArgumentCountStr = typeName.SubStr(backtickIndex + 1, genericArgumentStartIndex - backtickIndex - 1);
        if (!genericArgumentCountStr.TryParseAsInt(genericArgumentCount) || genericArgumentCount < 1)
            return NULL;

        utils::StringView<Il2CppNativeChar> typeDefinitionName = typeName.SubStr(0, genericArgumentStartIndex);
        Il2CppClass* classDefinition = GetClassFromMetadataTypeName(typeDefinitionName, false);
        if (classDefinition == NULL || !classDefinition->is_generic)
            return NULL;

        std::vector<const Il2CppType*> genericArguments;
        genericArguments.reserve(genericArgumentCount);

        int genericDepth = 0;
        const Il2CppNativeChar* genericArgumentsPtr = typeName.Str() + genericArgumentStartIndex + 1;
        const Il2CppNativeChar* currentGenericArgumentStart = genericArgumentsPtr;
        const Il2CppNativeChar* genericArgumentsEnd = typeName.Str() + typeName.Length() - 1;

        for (; genericArgumentsPtr <= genericArgumentsEnd; genericArgumentsPtr++)
        {
            Il2CppNativeChar currentChar = *genericArgumentsPtr;

            switch (currentChar)
            {
                case '<':
                    genericDepth++;
                    break;

                case '>':
                    if (genericArgumentsPtr < genericArgumentsEnd)
                    {
                        genericDepth--;
                        break;
                    }
                // fallthrough

                case ',':
                {
                    if (genericDepth == 0)
                    {
                        il2cpp::utils::StringView<Il2CppNativeChar> genericArgumentTypeName(currentGenericArgumentStart, genericArgumentsPtr - currentGenericArgumentStart);
                        Il2CppClass* genericArgumentClass = GetClassFromPrimitiveOrMetadataTypeName(genericArgumentTypeName, false);
                        if (genericArgumentClass == NULL)
                            return NULL;

                        genericArguments.push_back(&genericArgumentClass->byval_arg);
                        currentGenericArgumentStart = genericArgumentsPtr + 1;
                    }
                }
            }
        }

        if (genericArguments.size() != genericArgumentCount)
            return NULL;

        const Il2CppGenericInst* genericInst = MetadataCache::GetGenericInst(genericArguments);
        Il2CppGenericClass* genericClass = metadata::GenericMetadata::GetGenericClass(classDefinition, genericInst);
        return GenericClass::GetClass(genericClass);
    }

    static Il2CppClass* GetClassFromMetadataTypeName(utils::StringView<Il2CppNativeChar> typeName, bool throwOnFailure)
    {
        // Does this type involve generics?
        if (typeName[typeName.Length() - 1] == '>')
        {
            // Is it an array/boxed value?
            if (utils::StringUtils::StartsWith(typeName, kArrayTypeOrIReferencePrefix))
            {
                if (utils::StringUtils::StartsWith(typeName.SubStr(IL2CPP_ARRAY_SIZE(kArrayTypeOrIReferencePrefix) - 1), kArrayTypePostprefix))
                {
                    // We have an array
                    utils::StringView<Il2CppNativeChar> elementTypeName = typeName.SubStr(IL2CPP_ARRAY_SIZE(kArrayTypePrefix) - 1, typeName.Length() - IL2CPP_ARRAY_SIZE(kArrayTypePrefix));
                    Il2CppClass* elementClass = GetClassFromPrimitiveOrMetadataTypeName(elementTypeName, false);
                    if (elementClass != NULL)
                        return Class::GetArrayClass(elementClass, 1);
                }
                else if (utils::StringUtils::StartsWith(typeName.SubStr(IL2CPP_ARRAY_SIZE(kArrayTypeOrIReferencePrefix) - 1), kIReferencePostprefix))
                {
                    // We have a boxed value
                    utils::StringView<Il2CppNativeChar> boxedTypeName = typeName.SubStr(IL2CPP_ARRAY_SIZE(kIReferencePrefix) - 1, typeName.Length() - IL2CPP_ARRAY_SIZE(kIReferencePrefix));
                    Il2CppClass* boxedClass = GetClassFromPrimitiveOrMetadataTypeName(boxedTypeName, false);
                    if (boxedClass != NULL)
                    {
                        const Il2CppType* boxedType = &boxedClass->byval_arg;
                        const Il2CppGenericInst* genericInst = MetadataCache::GetGenericInst(&boxedType, 1);
                        Il2CppGenericClass* genericClass = metadata::GenericMetadata::GetGenericClass(il2cpp_defaults.generic_nullable_class, genericInst);
                        return GenericClass::GetClass(genericClass);
                    }
                }
            }

            // This could be a generic type
            Il2CppClass* klass = GetGenericInstanceClassFromMetadataTypeName(typeName);
            if (klass != NULL)
                return klass;
        }

        // It's not an generic array, or boxed type. Look in Windows Runtime class type map
        const std::string typeNameUtf8 = utils::StringUtils::NativeStringToUtf8(typeName.Str(), static_cast<uint32_t>(typeName.Length()));
        Il2CppClass* windowsRuntimeClass = MetadataCache::GetWindowsRuntimeClass(typeNameUtf8.c_str());
        if (windowsRuntimeClass != NULL)
            return windowsRuntimeClass;

        // We don't have it in Windows Runtime class type map. Look in WindowsRuntimeMetadata assembly
        size_t lastDotIndex = typeNameUtf8.rfind('.');
        if (lastDotIndex != std::string::npos)
        {
            const std::string namespaze = typeNameUtf8.substr(0, lastDotIndex);
            const char* name = typeNameUtf8.c_str() + lastDotIndex + 1;

            const Il2CppAssembly* windowsRuntimeMetadataAssembly = Assembly::Load("WindowsRuntimeMetadata");
            if (windowsRuntimeMetadataAssembly != NULL)
            {
                Il2CppClass* windowsRuntimeClass = Image::ClassFromName(windowsRuntimeMetadataAssembly->image, namespaze.c_str(), name);
                if (windowsRuntimeClass != NULL)
                    return windowsRuntimeClass;
            }
        }

        if (throwOnFailure)
        {
            // We couldn't find metadata type with given name, so we must now throw an exception.
            // Here's the catch: if a type name is actually a primitive type name, we need to
            // throw a special saying that the type kind was unexpected. Otherwise, we need to
            // throw the same exception as when we cannot find a primitive type.
            if (GetClassFromPrimitiveTypeName(typeName, false) != NULL)
                ThrowUnexpectedTypeKindException();

            // We want to start the generic part of the name from the exception message
            if (typeName[typeName.Length() - 1] == '>')
            {
                size_t genericArgumentStart = typeName.Find('<');
                if (genericArgumentStart != utils::StringView<Il2CppNativeChar>::NPos())
                    typeName = typeName.SubStr(0, genericArgumentStart);
            }

            ThrowWindowsRuntimeTypeNotFoundException(typeName);
        }

        return NULL;
    }

    static const Il2CppType* GetTypeFromCustomTypeName(utils::StringView<Il2CppNativeChar> typeName)
    {
        const std::string str = utils::StringUtils::NativeStringToUtf8(typeName.Str(), static_cast<uint32_t>(typeName.Length()));

        TypeNameParseInfo info;
        TypeNameParser parser(str, info, false);

        if (!parser.Parse())
        {
            utils::StringView<char>
            name(utils::StringView<char>::Empty()),
            assemblyName(utils::StringView<char>::Empty());

            size_t commaIndex = str.find_last_of(',');
            if (commaIndex != std::string::npos)
            {
                name = utils::StringView<char>(str.c_str(), commaIndex);
                while (commaIndex < str.length() && (str[commaIndex] == ' ' || str[commaIndex] == '\t'))
                    commaIndex++;

                if (commaIndex < str.length())
                    assemblyName = utils::StringView<char>(str.c_str() + commaIndex + 1, str.length() - commaIndex - 1);
            }
            else
            {
                name = STRING_TO_STRINGVIEW(str);
            }

            // Splitting name and namespace is pretty complicated, and they're going to be mashed up together in
            // the type load exception message anyway. Let's not bother.
            Exception::Raise(Exception::GetTypeLoadException(utils::StringView<char>::Empty(), name, assemblyName));
        }

        return Class::il2cpp_type_from_type_info(info, static_cast<TypeSearchFlags>(kTypeSearchFlagThrowOnError | kTypeSearchFlagDontUseExecutingImage));
    }

    const Il2CppType* WindowsRuntime::MarshalTypeFromNative(Il2CppWindowsRuntimeTypeName& nativeType)
    {
        if (nativeType.typeName == NULL)
            return NULL;

        uint32_t typeNameLength;
        const Il2CppNativeChar* typeNamePtr = os::WindowsRuntime::GetNativeHStringBuffer(nativeType.typeName, &typeNameLength);
        utils::StringView<Il2CppNativeChar> typeNameView(typeNamePtr, typeNameLength);

        switch (nativeType.typeKind)
        {
            case kTypeKindPrimitive:
                return &GetClassFromPrimitiveTypeName(typeNameView, true)->byval_arg;

            case kTypeKindMetadata:
                return &GetClassFromMetadataTypeName(typeNameView, true)->byval_arg;

            case kTypeKindCustom:
                return GetTypeFromCustomTypeName(typeNameView);

            default:
                ThrowUnexpectedTypeKindException();
        }
    }
}
}
