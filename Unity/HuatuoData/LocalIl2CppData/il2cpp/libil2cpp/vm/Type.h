#pragma once

#include <stdint.h>
#include <string>
#include <list>
#include <vector>
#include <algorithm>

#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"

#ifdef major
# undef major
# undef minor
#endif

struct FieldInfo;
struct Il2CppType;
struct Il2CppClass;
struct Il2CppString;

namespace il2cpp
{
namespace vm
{
    static const int32_t kPublicKeyTokenLength = 17;

    class TypeNameParseInfo
    {
    public:
        struct AssemblyName
        {
            std::string name;
            std::string culture;
            std::string public_key;
            char public_key_token[kPublicKeyTokenLength];
            uint32_t hash_alg;
            uint32_t hash_len;
            uint32_t flags;
            uint16_t major;
            uint16_t minor;
            uint16_t build;
            uint16_t revision;

            AssemblyName() :
                hash_alg(0),
                hash_len(0),
                flags(0),
                major(0),
                minor(0),
                build(0),
                revision(0)
            {
                memset(public_key_token, 0, kPublicKeyTokenLength);
            }
        };

        TypeNameParseInfo();
        ~TypeNameParseInfo();

        inline const std::string &ns() const
        {
            return _namespace;
        }

        inline const std::string &name() const
        {
            return _name;
        }

        inline const AssemblyName &assembly_name() const
        {
            return _assembly_name;
        }

        inline const std::vector<int> &modifiers() const
        {
            return _modifiers;
        }

        inline const std::vector<TypeNameParseInfo> &type_arguments() const
        {
            return _type_arguments;
        }

        inline const std::vector<std::string> &nested() const
        {
            return _nested;
        }

        inline bool is_byref() const
        {
            return std::find(_modifiers.begin(), _modifiers.end(), 0) != _modifiers.end();
        }

        inline bool has_generic_arguments() const
        {
            return _type_arguments.size() > 0;
        }

        inline bool is_pointer() const
        {
            return std::find(_modifiers.begin(), _modifiers.end(), -1) != _modifiers.end();
        }

        inline bool is_bounded() const
        {
            return std::find(_modifiers.begin(), _modifiers.end(), -2) != _modifiers.end();
        }

        inline bool is_array() const
        {
            std::vector<int32_t>::const_iterator it = _modifiers.begin();
            while (it != _modifiers.end())
            {
                if (*it > 0)
                    return true;

                ++it;
            }

            return false;
        }

        void SetAssemblyName(const AssemblyName& assemblyName)
        {
            _assembly_name = assemblyName;
        }

    private:

        std::string _namespace;
        std::string _name;
        AssemblyName _assembly_name;
        std::vector<int32_t> _modifiers;
        std::vector<TypeNameParseInfo> _type_arguments;
        std::vector<std::string> _nested;

        friend class TypeNameParser;
    };

    class TypeNameParser
    {
    public:

        TypeNameParser(const std::string &name, TypeNameParseInfo &info, bool is_nested);
        TypeNameParser(std::string::const_iterator &begin, std::string::const_iterator &end, TypeNameParseInfo &info, bool is_nested);

        bool Parse(bool acceptAssemblyName = true);
        bool ParseAssembly();

    private:

        inline bool IsEOL() const
        {
            return _p >= _end;
        }

        inline bool CurrentIs(char v) const
        {
            if (IsEOL())
                return false;

            return *_p == v;
        }

        inline bool Next(bool skipWhites = false)
        {
            ++_p;

            if (skipWhites)
                SkipWhites();

            return !IsEOL();
        }

        bool NextWillBe(char v, bool skipWhites = false) const;

        void InitializeParser();
        void SkipWhites();
        void ConsumeIdentifier();
        void ConsumeAssemblyIdentifier();
        void ConsumePropertyIdentifier();
        void ConsumePropertyValue();
        bool ConsumeNumber(int32_t &value);
        bool ParseTypeName(int32_t &arity);
        bool ParseNestedTypeOptional(int32_t &arity);
        bool ParseTypeArgumentsOptional(int32_t &arity);
        bool ParseAssemblyNameOptional();
        bool ParseAssemblyName();
        bool ParsePropertiesOptional();
        bool ParseArrayModifierOptional();
        bool ParsePointerModifiersOptional();
        bool ParseByRefModifiersOptional();

        static bool ParseVersion(const std::string& version, uint16_t& major, uint16_t& minor, uint16_t& build, uint16_t& revision);

        TypeNameParseInfo &_info;

        bool _is_nested;
        bool _accept_assembly_name;
        std::string::const_iterator _p;
        std::string::const_iterator _end;
    };

    class LIBIL2CPP_CODEGEN_API Type
    {
    public:
        // exported
        static void GetNameChunkedRecurse(const Il2CppType * type, Il2CppTypeNameFormat format, void(*reportFunc)(void *data, void *userData), void * userData);
        static std::string GetName(const Il2CppType *type, Il2CppTypeNameFormat format);
        static int GetType(const Il2CppType *type);
        static Il2CppClass* GetClassOrElementClass(const Il2CppType *type);
        static const Il2CppType* GetUnderlyingType(const Il2CppType *type);
        static uint32_t GetToken(const Il2CppType *type);
        static bool IsGenericInstance(const Il2CppType *type);
        static Il2CppReflectionType* GetDeclaringType(const Il2CppType* type);
        static Il2CppArray* GetGenericArgumentsInternal(Il2CppReflectionType* type, bool runtimeArray);
        static bool IsEqualToType(const Il2CppType *type, const Il2CppType *otherType);
        static Il2CppReflectionType* GetTypeFromHandle(intptr_t handle);

    public:
        // internal
        static void GetNameChunkedRecurseInternal(const Il2CppType * type, Il2CppTypeNameFormat format, bool is_nested, void(*reportFunc)(void *data, void *userData), void * userData);
        static void GetNameInternal(std::string &oss, const Il2CppType *type, Il2CppTypeNameFormat format, bool is_nested);
        static bool IsReference(const Il2CppType* type);
        static bool IsStruct(const Il2CppType* type);
        static bool GenericInstIsValuetype(const Il2CppType* type);

        static bool IsEnum(const Il2CppType *type);
        static bool IsValueType(const Il2CppType *type);
        static bool IsEmptyType(const Il2CppType *type);

        static bool IsSystemDBNull(const Il2CppType *type);
        static bool IsSystemDateTime(const Il2CppType *type);
        static bool IsSystemDecimal(const Il2CppType *type);

        static Il2CppClass* GetClass(const Il2CppType *type);
        static Il2CppMetadataGenericParameterHandle GetGenericParameterHandle(const Il2CppType *type);
        static Il2CppGenericParameterInfo GetGenericParameterInfo(const Il2CppType *type);
        static const Il2CppType* GetGenericTypeDefintion(const Il2CppType* type);

        static void ConstructDelegate(Il2CppDelegate* delegate, Il2CppObject* target, Il2CppMethodPointer addr, const MethodInfo* method);

        static Il2CppString* AppendAssemblyNameIfNecessary(Il2CppString* typeName, const MethodInfo* callingMethod);
    };
} /* namespace vm */
} /* namespace il2cpp */
