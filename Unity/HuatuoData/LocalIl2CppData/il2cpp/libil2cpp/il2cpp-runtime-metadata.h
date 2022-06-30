#pragma once

#include <stdint.h>
#include "il2cpp-blob.h"
#include "il2cpp-metadata.h"

struct Il2CppClass;
struct MethodInfo;
struct Il2CppType;

typedef struct Il2CppArrayType
{
    const Il2CppType* etype;
    uint8_t rank;
    uint8_t numsizes;
    uint8_t numlobounds;
    int *sizes;
    int *lobounds;
} Il2CppArrayType;

typedef struct Il2CppGenericInst
{
    uint32_t type_argc;
    const Il2CppType **type_argv;
} Il2CppGenericInst;

typedef struct Il2CppGenericContext
{
    /* The instantiation corresponding to the class generic parameters */
    const Il2CppGenericInst *class_inst;
    /* The instantiation corresponding to the method generic parameters */
    const Il2CppGenericInst *method_inst;
} Il2CppGenericContext;

typedef struct Il2CppGenericClass
{
    const Il2CppType* type;        /* the generic type definition */
    Il2CppGenericContext context;  /* a context that contains the type instantiation doesn't contain any method instantiation */
    Il2CppClass *cached_class;     /* if present, the Il2CppClass corresponding to the instantiation.  */
} Il2CppGenericClass;

typedef struct Il2CppGenericMethod
{
    const MethodInfo* methodDefinition;
    Il2CppGenericContext context;
} Il2CppGenericMethod;

typedef struct Il2CppType
{
    union
    {
        // We have this dummy field first because pre C99 compilers (MSVC) can only initializer the first value in a union.
        void* dummy;
        TypeDefinitionIndex __klassIndex; /* for VALUETYPE and CLASS at startup */
        Il2CppMetadataTypeHandle typeHandle; /* for VALUETYPE and CLASS at runtime */
        const Il2CppType *type;   /* for PTR and SZARRAY */
        Il2CppArrayType *array; /* for ARRAY */
        //MonoMethodSignature *method;
        GenericParameterIndex __genericParameterIndex; /* for VAR and MVAR at startup */
        Il2CppMetadataGenericParameterHandle genericParameterHandle; /* for VAR and MVAR at runtime */
        Il2CppGenericClass *generic_class; /* for GENERICINST */
    } data;
    unsigned int attrs    : 16; /* param attributes or field flags */
    Il2CppTypeEnum type     : 8;
    unsigned int num_mods : 5;  /* max 64 modifiers follow at the end */
    unsigned int byref    : 1;
    unsigned int pinned   : 1;  /* valid when included in a local var signature */
    unsigned int valuetype : 1;
    //MonoCustomMod modifiers [MONO_ZERO_LEN_ARRAY]; /* this may grow */
} Il2CppType;

typedef struct Il2CppMetadataFieldInfo
{
    const Il2CppType* type;
    const char* name;
    uint32_t token;
} Il2CppMetadataFieldInfo;

typedef struct Il2CppMetadataMethodInfo
{
    Il2CppMetadataMethodDefinitionHandle handle;
    const char* name;
    const Il2CppType* return_type;
    uint32_t token;
    uint16_t flags;
    uint16_t iflags;
    uint16_t slot;
    uint16_t parameterCount;
} Il2CppMetadataMethodInfo;

typedef struct Il2CppMetadataParameterInfo
{
    const char* name;
    uint32_t token;
    const Il2CppType* type;
} Il2CppMetadataParameterInfo;

typedef struct Il2CppMetadataPropertyInfo
{
    const char* name;
    const MethodInfo* get;
    const MethodInfo* set;
    uint32_t attrs;
    uint32_t token;
} Il2CppMetadataPropertyInfo;

typedef struct Il2CppMetadataEventInfo
{
    const char* name;
    const Il2CppType* type;
    const MethodInfo* add;
    const MethodInfo* remove;
    const MethodInfo* raise;
    uint32_t token;
} Il2CppMetadataEventInfo;

typedef struct Il2CppInterfaceOffsetInfo
{
    const Il2CppType* interfaceType;
    int32_t offset;
} Il2CppInterfaceOffsetInfo;


typedef struct Il2CppGenericParameterInfo
{
    Il2CppMetadataGenericContainerHandle containerHandle;
    const char* name;
    uint16_t num;
    uint16_t flags;
} Il2CppGenericParameterInfo;
