#pragma once

#include "utils/dynamic_array.h"
#include "il2cpp-object-internals.h"

struct MonoGPtrArray
{
    void *pdata;
    unsigned int len;
};

typedef il2cpp::utils::dynamic_array<void*> VoidPtrArray;

MonoGPtrArray* void_ptr_array_to_gptr_array(const VoidPtrArray& array);
MonoGPtrArray* empty_gptr_array();
void free_gptr_array(MonoGPtrArray *pArray);

struct MonoGenericParameterInfo
{
    Il2CppClass *pklass;        /* The corresponding `MonoClass'. */
    const char *name;

    // See GenericParameterAttributes
    unsigned short flags;

    unsigned int token;

    // Constraints on type parameters
    Il2CppClass** constraints; /* NULL means end of list */
};

struct  PublicKeyTokenFixedBuffer
{
    union
    {
        struct
        {
            // System.Byte Mono.MonoAssemblyName/<public_key_token>__FixedBuffer0::FixedElementField
            uint8_t FixedElementField;
        };
        uint8_t padding[17];
    };
};

struct  Il2CppMonoAssemblyName
{
    // System.IntPtr Mono.MonoAssemblyName::name
    const char * name;
    // System.IntPtr Mono.MonoAssemblyName::culture
    const char * culture;
    // System.IntPtr Mono.MonoAssemblyName::hash_value
    const char * hash_value;
    // System.IntPtr Mono.MonoAssemblyName::public_key
    const uint8_t* public_key;
    // Mono.MonoAssemblyName/<public_key_token>__FixedBuffer0 Mono.MonoAssemblyName::public_key_token
    PublicKeyTokenFixedBuffer  public_key_token;
    // System.UInt32 Mono.MonoAssemblyName::hash_alg
    uint32_t hash_alg;
    // System.UInt32 Mono.MonoAssemblyName::hash_len
    uint32_t hash_len;
    // System.UInt32 Mono.MonoAssemblyName::flags
    uint32_t flags;
    // System.UInt16 Mono.MonoAssemblyName::major
    uint16_t major;
    // System.UInt16 Mono.MonoAssemblyName::minor
    uint16_t minor;
    // System.UInt16 Mono.MonoAssemblyName::build
    uint16_t build;
    // System.UInt16 Mono.MonoAssemblyName::revision
    uint16_t revision;
    // System.UInt16 Mono.MonoAssemblyName::arch
    uint16_t arch;
};
