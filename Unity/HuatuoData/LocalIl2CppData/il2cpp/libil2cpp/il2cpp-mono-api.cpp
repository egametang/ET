// This file implements the Mono embedding API that the debugger code requires.
// It should not include any Mono headers.

#include "il2cpp-config.h"

#include "il2cpp-api.h"
#include "il2cpp-mono-api.h"

#include "il2cpp-class-internals.h"

#include "gc/GCHandle.h"
#include "gc/WriteBarrier.h"

#include "metadata/GenericMetadata.h"

#include "vm/Assembly.h"
#include "vm/Class.h"
#include "vm/Domain.h"
#include "vm/Field.h"
#include "vm/GenericClass.h"
#include "vm/GenericContainer.h"
#include "vm/Image.h"
#include "vm/MetadataCache.h"
#include "vm/Method.h"
#include "vm/Object.h"
#include "vm/Property.h"
#include "vm/Reflection.h"
#include "vm/Runtime.h"
#include "vm/String.h"
#include "vm/Thread.h"
#include "vm/ThreadPoolMs.h"
#include "vm/Type.h"

#include "vm-utils/Debugger.h"

#include "utils/Il2CppHashMap.h"
#include "utils/Memory.h"

#include <cstring>

#if IL2CPP_TARGET_XBOXONE
#define strdup _strdup
#endif

// These types must match the layout of types defined in Mono headers

struct Il2CppGPtrArray
{
    void** pdata;
    uint32_t len;
};

struct Il2CppGPtrArrayPriv
{
    void** pdata;
    uint32_t len;
    uint32_t size;
};

struct Il2CppMonoError
{
    unsigned short error_code;
    unsigned short flags;

    void *hidden_1[12];
};

struct Il2CppMonoMethodSignature
{
    Il2CppType *ret;
    uint16_t param_count;
    int16_t sentinalpos_unused;
    uint32_t generic_param_count        : 16;
    uint32_t call_convention_unused     : 6;
    uint32_t hasthis                    : 1;
    uint32_t explicit_this_unused       : 1;
    uint32_t pinvoke_unused             : 1;
    uint32_t is_inflated_unused         : 1;
    uint32_t has_type_parameters_unused : 1;
    Il2CppType* params[IL2CPP_ZERO_LEN_ARRAY];
};

struct Il2CppMonoMethodHeader
{
    const unsigned char* code_unused;
    uint32_t code_size;
    uint16_t max_stack_unused   : 15;
    uint32_t is_transient_unsued : 1;
    uint32_t num_clauses_unused : 15;
    uint32_t init_locals_unused : 1;
    uint16_t      num_locals;
    void* clauses_unused; // Actually a MonoExceptionClause in Mono,but we don't use it
    Il2CppType* locals[IL2CPP_ZERO_LEN_ARRAY];
};

struct Il2CppMonoDebugCodeBlock
{
    int32_t parent;
    int32_t type;
    int32_t start_offset;
    int32_t end_offset;
};

struct Il2CppMonoDebugLocalVar
{
    char *name;
    int32_t index;
    Il2CppMonoDebugCodeBlock *block;
};

struct Il2CppMonoDebugLocalsInfo
{
    int32_t num_locals;
    Il2CppMonoDebugLocalVar *locals;
    int32_t num_blocks;
    Il2CppMonoDebugCodeBlock *code_blocks;
};

struct Il2CppMonoDebugLineNumberEntry
{
    uint32_t il_offset;
    uint32_t native_offset;
};

struct Il2CppMonoDebugVarInfo
{
    uint32_t index;
    uint32_t offset;
    uint32_t size;
    uint32_t begin_scope;
    uint32_t end_scope;
    Il2CppType *type;
};

struct Il2CppMonoDebugMethodJitInfo
{
    const uint8_t *code_start;
    uint32_t code_size;
    uint32_t prologue_end;
    uint32_t epilogue_begin;
    const uint8_t *wrapper_addr;
    uint32_t num_line_numbers;
    Il2CppMonoDebugLineNumberEntry *line_numbers;
    uint32_t has_var_info;
    uint32_t num_params;
    Il2CppMonoDebugVarInfo *this_var;
    Il2CppMonoDebugVarInfo *params;
    uint32_t num_locals;
    Il2CppMonoDebugVarInfo *locals;
    Il2CppMonoDebugVarInfo *gsharedvt_info_var;
    Il2CppMonoDebugVarInfo *gsharedvt_locals_var;
};

enum
{
    BFLAGS_IgnoreCase = 1,
    BFLAGS_DeclaredOnly = 2,
    BFLAGS_Instance = 4,
    BFLAGS_Static = 8,
    BFLAGS_Public = 0x10,
    BFLAGS_NonPublic = 0x20,
    BFLAGS_FlattenHierarchy = 0x40,
    BFLAGS_InvokeMethod = 0x100,
    BFLAGS_CreateInstance = 0x200,
    BFLAGS_GetField = 0x400,
    BFLAGS_SetField = 0x800,
    BFLAGS_GetProperty = 0x1000,
    BFLAGS_SetProperty = 0x2000,
    BFLAGS_ExactBinding = 0x10000,
    BFLAGS_SuppressChangeType = 0x20000,
    BFLAGS_OptionalParamBinding = 0x40000
};

struct Il2CppMonoTypeNameParse
{
    char *name_space_unused;
    char *name_unused;
    Il2CppAssemblyName assembly;
    void *il2cppTypeNameParseInfo; // really GList *modifiers, but IL2CPP re-uses this field
    void *type_arguments_unused;
    void *nested_unused;
};

struct Il2CppMonoJitExceptionInfo
{
    uint32_t  flags;
    int32_t   exvar_offset;
    void* try_start;
    void* try_end;
    void* handler_start;
    /*
     * For LLVM compiled code, this is the index of the il clause
     * associated with this handler.
     */
    int clause_index;
    uint32_t try_offset;
    uint32_t try_len;
    uint32_t handler_offset;
    uint32_t handler_len;
    union
    {
        MonoClass *catch_class;
        void* filter;
        void* handler_end;
    } data;
};

struct Il2CppMonoJitInfo
{
    /* NOTE: These first two elements (method and
       next_jit_code_hash) must be in the same order and at the
       same offset as in RuntimeMethod, because of the jit_code_hash
       internal hash table in MonoDomain. */
    union
    {
        MonoMethod *method;
        MonoImage *image;
        void* aot_info;
        void* tramp_info;
    } d;
    union
    {
        void *next_jit_code_hash;
        void *next_tombstone;
    } n;
    void*    code_start;
    uint32_t     unwind_info;
    int         code_size;
    uint32_t     num_clauses : 15;
    /* Whenever the code is domain neutral or 'shared' */
    int32_t    domain_neutral : 1;
    int32_t    has_generic_jit_info : 1;
    int32_t    has_try_block_holes : 1;
    int32_t    has_arch_eh_info : 1;
    int32_t    has_thunk_info : 1;
    int32_t    has_unwind_info : 1;
    int32_t    from_aot : 1;
    int32_t    from_llvm : 1;
    int32_t    dbg_attrs_inited : 1;
    int32_t    dbg_hidden : 1;
    /* Whenever this jit info was loaded in async context */
    int32_t    async : 1;
    int32_t    dbg_step_through : 1;
    int32_t    dbg_non_user_code : 1;
    /*
     * Whenever this jit info refers to a trampoline.
     * d.tramp_info contains additional data in this case.
     */
    int32_t    is_trampoline : 1;
    /* Whenever this jit info refers to an interpreter method */
    int32_t    is_interp : 1;

    /* FIXME: Embed this after the structure later*/
    void*    gc_info; /* Currently only used by SGen */

    Il2CppMonoJitExceptionInfo clauses[IL2CPP_ZERO_LEN_ARRAY];
    /* There is an optional MonoGenericJitInfo after the clauses */
    /* There is an optional MonoTryBlockHoleTableJitInfo after MonoGenericJitInfo clauses*/
    /* There is an optional MonoArchEHJitInfo after MonoTryBlockHoleTableJitInfo */
    /* There is an optional MonoThunkJitInfo after MonoArchEHJitInfo */
};

// End of mirrored types

static void initialize_il2cpp_mono_method_signature(Il2CppMonoMethodSignature* signature, MethodInfo* method)
{
    signature->hasthis = il2cpp::vm::Method::IsInstance(method);
    signature->ret = (Il2CppType*)il2cpp::vm::Method::GetReturnType(method);

    signature->generic_param_count = 0;

    if (method->is_generic)
    {
        signature->generic_param_count = il2cpp::vm::Method::GetGenericParamCount(method);
    }
    else if (method->is_inflated)
    {
        if (method->genericMethod->context.method_inst)
            signature->generic_param_count += method->genericMethod->context.method_inst->type_argc;

        if (method->genericMethod->context.class_inst)
            signature->generic_param_count += method->genericMethod->context.class_inst->type_argc;
    }
    signature->param_count = il2cpp::vm::Method::GetParamCount(method);
    for (int i = 0; i < signature->param_count; ++i)
        signature->params[i] = (Il2CppType*)il2cpp::vm::Method::GetParam(method, i);
}

// We need to allocate the Il2CppMonoMethodSignature struct with C-style allocators because it is
// a mirror of _MonoMethodSignature, which end in a zero-length array. So wrap it in this C++
// struct that we can insert into a hash map and get proper memory management.
struct Il2CppMonoMethodSignatureWrapper
{
    Il2CppMonoMethodSignatureWrapper(MethodInfo* method)
    {
        // We need the size of Il2CppMonoMethodSignature plus one pointer for each parameter of the method.
        size_t methodSignatureSize =  sizeof(Il2CppMonoMethodSignature) + (sizeof(Il2CppType*) * il2cpp::vm::Method::GetParamCount(method));
        signature = (Il2CppMonoMethodSignature*)IL2CPP_CALLOC(1, methodSignatureSize);

        initialize_il2cpp_mono_method_signature(signature, method);
    }

    ~Il2CppMonoMethodSignatureWrapper()
    {
        IL2CPP_FREE(signature);
    }

    Il2CppMonoMethodSignature* signature;
};

typedef Il2CppHashMap<MethodInfo*, Il2CppMonoMethodSignatureWrapper*, il2cpp::utils::PointerHash<MethodInfo> > MethodSignatureMap;
static MethodSignatureMap* method_signatures;

static void error_init(MonoError* error)
{
    auto il2CppError = (Il2CppMonoError*)error;
    il2CppError->error_code = 0;
    il2CppError->flags = 0;
}

uint32_t mono_image_get_entry_point(MonoImage *image)
{
    const MethodInfo* entryPoint = il2cpp::vm::Image::GetEntryPoint((Il2CppImage*)image);
    return entryPoint == NULL ? 0 : entryPoint->token;
}

const char* mono_image_get_filename(MonoImage *image)
{
    return il2cpp_image_get_filename((Il2CppImage *)image);
}

const char* mono_image_get_guid(MonoImage *image)
{
    return "00000000-0000-0000-0000-000000000000"; //IL2CPP doesn't have image GUIDs
}

int32_t mono_image_is_dynamic(MonoImage *image)
{
    return false;
}

MonoAssembly* mono_image_get_assembly(MonoImage *image)
{
    return (MonoAssembly*)il2cpp_image_get_assembly((Il2CppImage *)image);
}

const char* mono_image_get_name(MonoImage *image)
{
    return il2cpp_image_get_name((Il2CppImage *)image);
}

MonoDomain* mono_get_root_domain(void)
{
    return (MonoDomain*)il2cpp::vm::Domain::GetCurrent();
}

MonoDomain* mono_domain_get(void)
{
    return mono_get_root_domain();
}

int32_t mono_domain_set(MonoDomain *domain, int32_t force)
{
    IL2CPP_ASSERT(domain == mono_get_root_domain());
    return true;
}

void mono_domain_foreach(MonoDomainFunc func, void* user_data)
{
    func((MonoDomain*)mono_get_root_domain(), user_data);
}

void mono_domain_lock(MonoDomain* domain)
{
}

void mono_domain_unlock(MonoDomain* domain)
{
}

const MonoAssembly* mono_domain_get_corlib(MonoDomain *domain)
{
    return (MonoAssembly*)il2cpp::vm::Image::GetAssembly((Il2CppImage*)il2cpp_defaults.corlib);
}

MonoAssembly* mono_domain_get_assemblies_iter(MonoAppDomain *domain, void** iter)
{
    if (!iter)
        return NULL;

    il2cpp::vm::AssemblyVector* assemblies = il2cpp::vm::Assembly::GetAllAssemblies();

    if (!*iter)
    {
        il2cpp::vm::AssemblyVector::iterator *pIter = new il2cpp::vm::AssemblyVector::iterator();
        *pIter = assemblies->begin();
        *iter = pIter;
        return (MonoAssembly*)**pIter;
    }

    il2cpp::vm::AssemblyVector::iterator *pIter = (il2cpp::vm::AssemblyVector::iterator*)*iter;
    (*pIter)++;
    if (*pIter != assemblies->end())
    {
        return (MonoAssembly*)(**pIter);
    }
    else
    {
        delete pIter;
        *iter = NULL;
    }

    return NULL;
}

MonoClass* mono_type_get_class(MonoType *type)
{
    return (MonoClass*)il2cpp::vm::Type::GetClass((Il2CppType*)type);
}

MonoGenericClass* m_type_get_generic_class(MonoType* type)
{
    return (MonoGenericClass*)((Il2CppType*)type)->data.generic_class;
}

int32_t mono_type_is_struct(MonoType *type)
{
    return il2cpp::vm::Type::IsStruct((Il2CppType*)type);
}

int32_t mono_type_is_reference(MonoType *type)
{
    return il2cpp::vm::Type::IsReference((Il2CppType*)type);
}

int32_t mono_type_generic_inst_is_valuetype(MonoType *monoType)
{
    static const int kBitIsValueType = 1;
    Il2CppType *type = (Il2CppType*)monoType;
    Il2CppMetadataTypeHandle handle = il2cpp::vm::MetadataCache::GetTypeHandleFromType(type->data.generic_class->type);
    return il2cpp::vm::MetadataCache::TypeIsValueType(handle);
}

char* mono_type_full_name(MonoType* type)
{
    std::string name = il2cpp::vm::Type::GetName((Il2CppType*)type, IL2CPP_TYPE_NAME_FORMAT_FULL_NAME);
    return strdup(name.c_str());
}

char* mono_type_get_name_full(MonoType* type, MonoTypeNameFormat format)
{
    std::string name = il2cpp::vm::Type::GetName((Il2CppType*)type, (Il2CppTypeNameFormat)format);
    return strdup(name.c_str());
}

MonoReflectionType* mono_type_get_object_checked(MonoDomain* domain, MonoType* type, MonoError* error)
{
    error_init(error);
    return (MonoReflectionType*)il2cpp::vm::Reflection::GetTypeObject((const Il2CppType*)type);
}

int mono_type_get_type(MonoType* type)
{
    return il2cpp_type_get_type((const Il2CppType*)type);
}

int32_t mono_type_is_byref(MonoType* type)
{
    return il2cpp_type_is_byref((const Il2CppType*)type);
}

uint32_t mono_type_get_attrs(MonoType* type)
{
    return il2cpp_type_get_attrs((const Il2CppType*)type);
}

MonoVTable* mono_class_vtable(MonoDomain *domain, MonoClass *klass)
{
    return (MonoVTable*)((Il2CppClass*)klass)->vtable;
}

int32_t mono_class_instance_size(MonoClass *klass)
{
    il2cpp::vm::Class::Init((Il2CppClass*)klass);
    return il2cpp_class_instance_size((Il2CppClass*)klass);
}

int32_t mono_class_value_size(MonoClass *klass, uint32_t *align)
{
    return il2cpp::vm::Class::GetValueSize((Il2CppClass*)klass, align);
}

int32_t mono_class_is_assignable_from(MonoClass *klass, MonoClass *oklass)
{
    return il2cpp::vm::Class::IsAssignableFrom((Il2CppClass*)klass, (Il2CppClass*)oklass);
}

MonoClass* mono_class_from_mono_type(MonoType *type)
{
    return (MonoClass*)il2cpp::vm::Class::FromIl2CppType((Il2CppType*)type);
}

uint32_t mono_class_get_flags(MonoClass * klass)
{
    return il2cpp_class_get_flags((Il2CppClass*)klass);
}

int mono_class_num_fields(MonoClass *klass)
{
    return (int)il2cpp::vm::Class::GetNumFields((Il2CppClass*)klass);
}

int mono_class_num_methods(MonoClass *klass)
{
    return (int)il2cpp::vm::Class::GetNumMethods((Il2CppClass*)klass);
}

int mono_class_num_properties(MonoClass *klass)
{
    return (int)il2cpp::vm::Class::GetNumProperties((Il2CppClass*)klass);
}

MonoClassField* mono_class_get_fields(MonoClass* klass, void* *iter)
{
    return (MonoClassField*)il2cpp::vm::Class::GetFields((Il2CppClass*)klass, iter);
}

MonoMethod* mono_class_get_methods(MonoClass* klass, void* *iter)
{
    return (MonoMethod*)il2cpp::vm::Class::GetMethods((Il2CppClass*)klass, iter);
}

MonoProperty* mono_class_get_properties(MonoClass* klass, void* *iter)
{
    return (MonoProperty*)il2cpp::vm::Class::GetProperties((Il2CppClass*)klass, iter);
}

MonoClass* mono_class_get_nested_types(MonoClass *monoClass, void* *iter)
{
    Il2CppClass *klass = (Il2CppClass*)monoClass;
    if (klass->generic_class)
        return NULL;

    return (MonoClass*)il2cpp::vm::Class::GetNestedTypes(klass, iter);
}

void mono_class_setup_methods(MonoClass* klass)
{
    il2cpp::vm::Class::SetupMethods((Il2CppClass*)klass);
}

void mono_class_setup_vtable(MonoClass* klass)
{
    il2cpp::vm::Class::Init((Il2CppClass*)klass);
}

static int32_t method_nonpublic(MethodInfo* method, int32_t start_klass)
{
    switch (method->flags & METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK)
    {
        case METHOD_ATTRIBUTE_ASSEM:
            return (start_klass || il2cpp_defaults.generic_ilist_class);
        case METHOD_ATTRIBUTE_PRIVATE:
            return start_klass;
        case METHOD_ATTRIBUTE_PUBLIC:
            return false;
        default:
            return true;
    }
}

static Il2CppGPtrArray* il2cpp_g_ptr_array_new()
{
    Il2CppGPtrArrayPriv* array = (Il2CppGPtrArrayPriv*)IL2CPP_CALLOC(1, sizeof(Il2CppGPtrArrayPriv));

    array->pdata = NULL;
    array->len = 0;
    array->size = 0;

    return (Il2CppGPtrArray *)array;
}

static void il2cpp_g_ptr_array_grow(Il2CppGPtrArrayPriv* array, uint32_t length)
{
    uint32_t new_length = array->len + length;

    IL2CPP_ASSERT(array != NULL);

    if (new_length <= array->size)
        return;

    array->size = 1;

    while (array->size < new_length)
        array->size <<= 1;

    array->size = std::max(array->size, 16U);
    array->pdata = (void**)IL2CPP_REALLOC(array->pdata, array->size * sizeof(void*));
}

static void il2cpp_g_ptr_array_add(Il2CppGPtrArray* array, void* data)
{
    IL2CPP_ASSERT(array != NULL);
    il2cpp_g_ptr_array_grow((Il2CppGPtrArrayPriv *)array, 1);
    array->pdata[array->len++] = data;
}

static int32_t il2cpp_g_ascii_strcasecmp(const char *s1, const char *s2)
{
    const char *sp1 = s1;
    const char *sp2 = s2;

    if (s1 == NULL)
        return 0;
    if (s2 == NULL)
        return 0;

    while (*sp1 != '\0')
    {
        char c1 = tolower(*sp1++);
        char c2 = tolower(*sp2++);

        if (c1 != c2)
            return c1 - c2;
    }

    return (*sp1) - (*sp2);
}

GPtrArray* mono_class_get_methods_by_name(MonoClass* il2cppMonoKlass, const char* name, uint32_t bflags, int32_t ignore_case, int32_t allow_ctors, MonoError* error)
{
#if IL2CPP_MONO_DEBUGGER
    Il2CppGPtrArray *array;
    Il2CppClass *klass = (Il2CppClass*)il2cppMonoKlass;
    Il2CppClass *startklass;
    MethodInfo *method;
    void* iter;
    int match;
    int (*compare_func) (const char *s1, const char *s2) = NULL;

    array = il2cpp_g_ptr_array_new();
    startklass = klass;
    error_init(error);

    if (name != NULL)
        compare_func = (ignore_case) ? il2cpp_g_ascii_strcasecmp : strcmp;

handle_parent:
    mono_class_setup_methods((MonoClass*)klass);
    mono_class_setup_vtable((MonoClass*)klass);

    iter = NULL;
    while ((method = (MethodInfo*)mono_class_get_methods((MonoClass*)klass, &iter)))
    {
        match = 0;

        if (!allow_ctors && method->name[0] == '.' && (strcmp(method->name, ".ctor") == 0 || strcmp(method->name, ".cctor") == 0))
            continue;
        if ((method->flags & METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK) == METHOD_ATTRIBUTE_PUBLIC)
        {
            if (bflags & BFLAGS_Public)
                match++;
        }
        else if ((bflags & BFLAGS_NonPublic) && method_nonpublic(method, (klass == startklass)))
        {
            match++;
        }
        if (!match)
            continue;
        match = 0;
        if (method->flags & METHOD_ATTRIBUTE_STATIC)
        {
            if (bflags & BFLAGS_Static)
                if ((bflags & BFLAGS_FlattenHierarchy) || (klass == startklass))
                    match++;
        }
        else
        {
            if (bflags & BFLAGS_Instance)
                match++;
        }

        if (!match)
            continue;

        if (name != NULL)
        {
            if (compare_func(name, method->name))
                continue;
        }

        match = 0;
        il2cpp_g_ptr_array_add(array, method);
    }

    if (!(bflags & BFLAGS_DeclaredOnly) && (klass = klass->parent))
        goto handle_parent;

    return (GPtrArray*)array;
#else
    return NULL;
#endif
}

MonoMethod* mono_class_get_method_from_name(MonoClass * klass, const char* name, int argsCount)
{
    return (MonoMethod*)il2cpp_class_get_method_from_name((Il2CppClass*)klass, name, argsCount);
}

int32_t mono_class_is_abstract(MonoClass * klass)
{
    return il2cpp_class_is_abstract((Il2CppClass*)klass);
}

int32_t mono_class_field_is_special_static(MonoClassField* field)
{
    return il2cpp::vm::Field::IsNormalStatic((FieldInfo*)field) ? 0 : 1;
}

MonoGenericContext* mono_class_get_context(MonoClass* klass)
{
    return (MonoGenericContext*)&((Il2CppClass*)klass)->generic_class->context;
}

MonoMethod* mono_class_inflate_generic_method_full_checked(MonoMethod* method, MonoClass* klass_hint, MonoGenericContext* context, MonoError* error)
{
    error_init(error);
    return (MonoMethod*)il2cpp::metadata::GenericMetadata::Inflate((MethodInfo*)method, (Il2CppGenericContext*)context);
}

MonoMethod* mono_class_inflate_generic_method_checked(MonoMethod* method, MonoGenericContext* context, MonoError* error)
{
    error_init(error);
    return (MonoMethod*)il2cpp::metadata::GenericMetadata::Inflate((MethodInfo*)method, (Il2CppGenericContext*)context);
}

int32_t mono_class_is_nullable(MonoClass* klass)
{
    return il2cpp::vm::Class::IsNullable((Il2CppClass*)klass);
}

MonoGenericContainer* mono_class_get_generic_container(MonoClass* klass)
{
    return (MonoGenericContainer*)il2cpp::vm::Class::GetGenericContainer((Il2CppClass*)klass);
}

void mono_class_setup_interfaces(MonoClass* klass, MonoError* error)
{
    error_init(error);
    il2cpp::vm::Class::SetupInterfaces((Il2CppClass*)klass);
}

int32_t mono_class_is_valuetype(MonoClass* klass)
{
    return il2cpp_class_is_valuetype((Il2CppClass*)klass);
}

MonoClass* mono_class_from_generic_parameter_internal(MonoGenericParam* param)
{
    return (MonoClass*)il2cpp::vm::Class::FromGenericParameter((Il2CppMetadataGenericParameterHandle)param);
}

MonoGenericClass* mono_class_get_generic_class(MonoClass* monoClass)
{
    Il2CppClass *klass = (Il2CppClass*)monoClass;
    return (MonoGenericClass*)klass->generic_class;
}

MonoClass* mono_class_try_load_from_name(MonoImage* image, const char* namespaze, const char* name)
{
    return (MonoClass*)il2cpp_class_from_name((const Il2CppImage*)image, namespaze, name);
}

int32_t mono_class_is_gtd(MonoClass* klass)
{
    return il2cpp_class_is_generic((Il2CppClass*)klass);
}

int32_t mono_class_is_ginst(MonoClass* klass)
{
    return il2cpp_class_is_inflated((Il2CppClass*)klass);
}

const char* mono_class_get_namespace(MonoClass * klass)
{
    return il2cpp_class_get_namespace((Il2CppClass*)klass);
}

const char* mono_class_get_name(MonoClass * klass)
{
    return il2cpp_class_get_name((Il2CppClass*)klass);
}

MonoClass* mono_class_get_parent(MonoClass * klass)
{
    return (MonoClass*)il2cpp_class_get_parent((Il2CppClass*)klass);
}

MonoType* mono_class_get_type(MonoClass * klass)
{
    return (MonoType*)il2cpp_class_get_type((Il2CppClass*)klass);
}

uint32_t mono_class_get_type_token(MonoClass * klass)
{
    return il2cpp_class_get_type_token((Il2CppClass*)klass);
}

MonoType* mono_class_get_byref_type(MonoClass *klass)
{
    return (MonoType*)il2cpp::vm::Class::GetByrefType((Il2CppClass*)klass);
}

MonoImage* mono_class_get_image(MonoClass * klass)
{
    return (MonoImage*)il2cpp_class_get_image((Il2CppClass*)klass);
}

MonoClass* mono_class_get_interfaces(MonoClass * klass, void* *iter)
{
    return (MonoClass*)il2cpp_class_get_interfaces((Il2CppClass*)klass, iter);
}

int32_t mono_class_is_interface(MonoClass * klass)
{
    return il2cpp_class_is_interface((Il2CppClass*)klass);
}

int mono_class_get_rank(MonoClass * klass)
{
    return il2cpp_class_get_rank((Il2CppClass*)klass);
}

MonoClass* mono_class_get_element_class(MonoClass * klass)
{
    return (MonoClass*)il2cpp_class_get_element_class((Il2CppClass*)klass);
}

int32_t mono_class_is_enum(MonoClass * klass)
{
    return il2cpp_class_is_enum((Il2CppClass*)klass);
}

MonoMethodSignature* mono_method_signature(MonoMethod *m)
{
    MethodInfo* method = (MethodInfo*)m;

    if (method_signatures == NULL)
        method_signatures = new MethodSignatureMap();

    auto entry = method_signatures->find(method);
    if (entry != method_signatures->end())
        return (MonoMethodSignature*)entry->second->signature;

    Il2CppMonoMethodSignatureWrapper* wrapper = new Il2CppMonoMethodSignatureWrapper(method);
    method_signatures->add(method, wrapper);

    return (MonoMethodSignature*)wrapper->signature;
}

void mono_free_method_signatures()
{
    delete method_signatures;
    method_signatures = NULL;
}

MonoDebugLocalsInfo* mono_debug_lookup_locals(MonoMethod *method)
{
#if IL2CPP_MONO_DEBUGGER
    uint32_t executionContextInfoCount;
    const Il2CppMethodExecutionContextInfo * executionContextInfo;
    const Il2CppMethodHeaderInfo *headerInfo;
    const Il2CppMethodScope *scopes;
    il2cpp::utils::Debugger::GetMethodExecutionContextInfo((const MethodInfo*)method, &executionContextInfoCount, &executionContextInfo, &headerInfo, &scopes);

    Il2CppMonoDebugLocalsInfo* locals = (Il2CppMonoDebugLocalsInfo*)IL2CPP_CALLOC(1, sizeof(Il2CppMonoDebugLocalsInfo));
    locals->num_locals = executionContextInfoCount;

    locals->locals = (Il2CppMonoDebugLocalVar*)IL2CPP_CALLOC(executionContextInfoCount, sizeof(Il2CppMonoDebugLocalVar));
    for (int i = 0; i < locals->num_locals; ++i)
    {
        locals->locals[i].name = (char*)il2cpp::utils::Debugger::GetLocalName((const MethodInfo*)method, executionContextInfo[i].nameIndex);
        locals->locals[i].index = i;

        /* hack we should point to blocks allocated below? */
        locals->locals[i].block = (Il2CppMonoDebugCodeBlock*)IL2CPP_CALLOC(1, sizeof(Il2CppMonoDebugCodeBlock));
        const Il2CppMethodScope* scope = il2cpp::utils::Debugger::GetLocalScope((const MethodInfo*)method, executionContextInfo[i].scopeIndex);
        locals->locals[i].block->start_offset = scope->startOffset;
        locals->locals[i].block->end_offset = scope->endOffset;
    }

    locals->num_blocks = headerInfo->numScopes;
    locals->code_blocks = (Il2CppMonoDebugCodeBlock*)IL2CPP_CALLOC(headerInfo->numScopes, sizeof(Il2CppMonoDebugCodeBlock));

    for (int i = 0; i < headerInfo->numScopes; ++i)
    {
        locals->code_blocks[i].start_offset = scopes[i].startOffset;
        locals->code_blocks[i].end_offset = scopes[i].endOffset;
    }

    return (MonoDebugLocalsInfo*)locals;
#else
    return NULL;
#endif
}

void mono_debug_free_locals(MonoDebugLocalsInfo *info)
{
#if IL2CPP_MONO_DEBUGGER
    Il2CppMonoDebugLocalsInfo* locals = (Il2CppMonoDebugLocalsInfo*)info;
    for (int i = 0; i < locals->num_locals; ++i)
    {
        IL2CPP_FREE(locals->locals[i].block);
    }
    IL2CPP_FREE(locals->locals);
    IL2CPP_FREE(locals->code_blocks);
    IL2CPP_FREE(locals);
#endif
}

MonoDebugMethodJitInfo* mono_debug_find_method(MonoMethod *method, MonoDomain *domain)
{
#if IL2CPP_MONO_DEBUGGER
    Il2CppMonoDebugMethodJitInfo* jit = (Il2CppMonoDebugMethodJitInfo*)IL2CPP_CALLOC(1, sizeof(Il2CppMonoDebugMethodJitInfo));
    Il2CppMonoDebugLocalsInfo* locals_info = (Il2CppMonoDebugLocalsInfo*)mono_debug_lookup_locals(method);
    jit->num_locals = locals_info->num_locals;

    Il2CppMonoMethodSignature* sig = (Il2CppMonoMethodSignature*)mono_method_signature(method);
    jit->num_params = sig->param_count;

    return (MonoDebugMethodJitInfo*)jit;
#else
    return NULL;
#endif
}

void mono_method_get_param_names(MonoMethod *m, const char **names)
{
    MethodInfo* method = (MethodInfo*)m;
    uint32_t numberOfParameters = il2cpp::vm::Method::GetParamCount(method);
    for (uint32_t i = 0; i < numberOfParameters; ++i)
        names[i] = il2cpp::vm::Method::GetParamName(method, i);
}

MonoGenericContext* mono_method_get_context(MonoMethod* monoMethod)
{
    MethodInfo* method = (MethodInfo*)monoMethod;

    if (!method->is_inflated || method->is_generic)
        return NULL;

    return (MonoGenericContext*)&((MethodInfo*)method)->genericMethod->context;
}

MonoMethodHeader* mono_method_get_header_checked(MonoMethod *method, MonoError *error)
{
#if IL2CPP_MONO_DEBUGGER
    if (error)
        error_init(error);

    uint32_t executionContextInfoCount;
    const Il2CppMethodExecutionContextInfo *executionContextInfo;
    const Il2CppMethodHeaderInfo *headerInfo;
    const Il2CppMethodScope *scopes;
    MonoGenericContext* context = mono_method_get_context(method);

    il2cpp::utils::Debugger::GetMethodExecutionContextInfo((const MethodInfo*)method, &executionContextInfoCount, &executionContextInfo, &headerInfo, &scopes);

    Il2CppMonoMethodHeader* header = (Il2CppMonoMethodHeader*)IL2CPP_CALLOC(1, sizeof(Il2CppMonoMethodHeader) + (executionContextInfoCount * sizeof(Il2CppType*)));
    header->code_size = headerInfo->code_size;
    header->num_locals = executionContextInfoCount;
    for (uint32_t i = 0; i < executionContextInfoCount; i++)
        header->locals[i] = (Il2CppType*)il2cpp::metadata::GenericMetadata::InflateIfNeeded(il2cpp::vm::MetadataCache::GetIl2CppTypeFromIndex(NULL, executionContextInfo[i].typeIndex), (const Il2CppGenericContext *)context, true);

    return (MonoMethodHeader*)header;
#else
    return NULL;
#endif
}

void mono_metadata_free_mh(MonoMethodHeader *mh)
{
    IL2CPP_FREE(mh);
}

char* mono_method_full_name(MonoMethod* method, int32_t signature)
{
    return strdup(((MethodInfo*)method)->name);
}

MonoGenericContainer* mono_method_get_generic_container(MonoMethod* monoMethod)
{
    MethodInfo * method = (MethodInfo*)monoMethod;

    if (method->is_inflated || !method->is_generic)
        return NULL;

    return (MonoGenericContainer*)method->genericContainerHandle;
}

void* mono_method_get_wrapper_data(MonoMethod* method, uint32_t id)
{
    IL2CPP_ASSERT(0 && "This method is not supported");
    return 0;
}

MonoMethod* mono_method_get_declaring_generic_method(MonoMethod* method)
{
    IL2CPP_ASSERT(0 && "This method is not supported");
    return NULL;
}

const char* mono_method_get_name(MonoMethod *method)
{
    return il2cpp::vm::Method::GetName((const MethodInfo*)method);
}

MonoClass* mono_method_get_class(MonoMethod *method)
{
    return (MonoClass*)il2cpp::vm::Method::GetClass((const MethodInfo*)method);
}

uint32_t mono_method_get_flags(MonoMethod *method, uint32_t *iflags)
{
    if (iflags != 0)
        *iflags = il2cpp::vm::Method::GetImplementationFlags((const MethodInfo*)method);

    return il2cpp::vm::Method::GetFlags((const MethodInfo*)method);
}

uint32_t mono_method_get_token(MonoMethod *method)
{
    return il2cpp::vm::Method::GetToken((const MethodInfo*)method);
}

bool mono_method_is_generic(MonoMethod *method)
{
    return il2cpp::vm::Method::IsGeneric((const MethodInfo*)method);
}

bool mono_method_is_inflated(MonoMethod *method)
{
    return il2cpp::vm::Method::IsInflated((const MethodInfo*)method);
}

int32_t mono_array_element_size(MonoClass *monoClass)
{
    Il2CppClass *klass = (Il2CppClass*)monoClass;
    return klass->element_size;
}

char* mono_array_addr_with_size(MonoArray *array, int size, uintptr_t idx)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return NULL;
}

uintptr_t mono_array_length(MonoArray *array)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return 0;
}

const char* mono_field_get_name(MonoClassField *field)
{
    return il2cpp::vm::Field::GetName((FieldInfo*)field);
}

void mono_field_set_value(MonoObject *obj, MonoClassField *field, void *value)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void mono_field_static_set_value(MonoVTable *vt, MonoClassField *field, void *value)
{
    il2cpp::vm::Field::StaticSetValue((FieldInfo*)field, value);
}

void mono_field_static_get_value_checked(MonoVTable* vt, MonoClassField* field, void* value, MonoError* error)
{
    error_init(error);
    il2cpp::vm::Field::StaticGetValue((FieldInfo*)field, value);
}

void mono_field_static_get_value_for_thread(MonoInternalThread* thread, MonoVTable* vt, MonoClassField* field, void* value, MonoError* error)
{
    error_init(error);
    il2cpp::vm::Field::StaticGetValueForThread((FieldInfo*)field, value, (Il2CppInternalThread*)thread);
}

MonoObject* mono_field_get_value_object_checked(MonoDomain* domain, MonoClassField* field, MonoObject* obj, MonoError* error)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return NULL;
}

MonoClass* mono_field_get_parent(MonoClassField *field)
{
    return (MonoClass*)il2cpp::vm::Field::GetParent((FieldInfo*)field);
}

uint32_t mono_field_get_offset(MonoClassField *field)
{
    return (uint32_t)il2cpp::vm::Field::GetOffset((FieldInfo*)field);
}

MonoType* mono_field_get_type(MonoClassField *field)
{
    return (MonoType*)il2cpp::vm::Field::GetType((FieldInfo*)field);
}

uint16_t* mono_string_chars(MonoString *monoStr)
{
    Il2CppString *str = (Il2CppString*)monoStr;
    return (uint16_t*)str->chars;
}

int mono_string_length(MonoString *monoStr)
{
    Il2CppString *str = (Il2CppString*)monoStr;
    return str->length;
}

MonoString* mono_string_new(MonoDomain *domain, const char *text)
{
    return (MonoString*)il2cpp::vm::String::New(text);
}

MonoString* mono_string_new_checked(MonoDomain *domain, const char *text, MonoError *merror)
{
    error_init(merror);
    return mono_string_new(domain, text);
}

char* mono_string_to_utf8_checked(MonoString *string_obj, MonoError *error)
{
    error_init(error);
    Il2CppString *str = (Il2CppString*)string_obj;
    std::string s = il2cpp::utils::StringUtils::Utf16ToUtf8(str->chars, str->length);
    return strdup(s.c_str());
}

int mono_object_hash(MonoObject* obj)
{
    return (int)((intptr_t)obj >> 3);
}

void* mono_object_unbox(MonoObject *monoObj)
{
    Il2CppObject *obj = (Il2CppObject*)monoObj;
    return il2cpp::vm::Object::Unbox(obj);
}

MonoMethod* mono_object_get_virtual_method(MonoObject *obj, MonoMethod *method)
{
    return (MonoMethod*)il2cpp::vm::Object::GetVirtualMethod((Il2CppObject*)obj, (const MethodInfo*)method);
}

MonoObject* mono_object_new_checked(MonoDomain* domain, MonoClass* klass, MonoError* error)
{
    error_init(error);
    return (MonoObject*)il2cpp::vm::Object::New((Il2CppClass*)klass);
}

MonoType* mono_object_get_type(MonoObject* object)
{
    return (MonoType*)&(((Il2CppObject*)object)->klass->byval_arg);
}

MonoClass* mono_object_get_class(MonoObject* obj)
{
    return (MonoClass*)il2cpp::vm::Object::GetClass((Il2CppObject*)obj);
}

MonoMethod* mono_get_method_checked(MonoImage* image, uint32_t token, MonoClass* klass, MonoGenericContext* context, MonoError* error)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return NULL;
}

uint32_t mono_gchandle_new_weakref(MonoObject *obj, int32_t track_resurrection)
{
    auto weakRef = il2cpp::gc::GCHandle::NewWeakref((Il2CppObject*)obj, track_resurrection == 0 ? false : true);
    il2cpp::vm::Exception::RaiseIfError(weakRef.GetError());
    return weakRef.Get();
}

MonoObject* mono_gchandle_get_target(uint32_t gchandle)
{
    return (MonoObject*)il2cpp::gc::GCHandle::GetTarget(gchandle);
}

void mono_gchandle_free(uint32_t gchandle)
{
    il2cpp::gc::GCHandle::Free(gchandle);
}

MonoThread* mono_thread_current()
{
    return (MonoThread*)il2cpp::vm::Thread::Current();
}

MonoThread* mono_thread_get_main()
{
    return (MonoThread*)il2cpp::vm::Thread::Main();
}

MonoThread* mono_thread_attach(MonoDomain* domain)
{
    return (MonoThread*)il2cpp::vm::Thread::Attach((Il2CppDomain*)domain);
}

void mono_thread_detach(MonoThread* thread)
{
    il2cpp::vm::Thread::Detach((Il2CppThread*)thread);
}

MonoInternalThread* mono_thread_internal_current()
{
    Il2CppThread* currentThread = (Il2CppThread*)mono_thread_current();
    if (currentThread == NULL)
        return NULL;
    return (MonoInternalThread*)currentThread->internal_thread;
}

int32_t mono_thread_internal_is_current(MonoInternalThread* thread)
{
    MonoInternalThread* currentThread = mono_thread_internal_current();
    if (currentThread == NULL)
        return false;
    return currentThread == thread;
}

void mono_thread_internal_abort(MonoInternalThread* thread, int32_t appdomain_unload)
{
    il2cpp::vm::Thread::RequestAbort((Il2CppInternalThread*)thread);
}

void mono_thread_internal_reset_abort(MonoInternalThread* thread)
{
    il2cpp::vm::Thread::ResetAbort((Il2CppInternalThread*)thread);
}

uint16_t* mono_thread_get_name(MonoInternalThread* this_obj, uint32_t* name_len)
{
    std::string name = il2cpp::vm::Thread::GetName((Il2CppInternalThread*)this_obj);

    auto numberOfCharacters = name.size();
    if (name_len != NULL)
        *name_len = (uint32_t)numberOfCharacters;

    if (name.empty())
        return NULL;

    auto utf16Name = il2cpp::utils::StringUtils::Utf8ToUtf16(name.c_str(), numberOfCharacters);

    size_t outputNameSize = utf16Name.size() * sizeof(uint16_t);
    uint16_t* outputName = (uint16_t*)IL2CPP_MALLOC(outputNameSize);

    std::memcpy(outputName, utf16Name.data(), outputNameSize);

    return outputName;
}

void mono_thread_set_name_internal(MonoInternalThread* this_obj, MonoString* name, int32_t permanent, int32_t reset, MonoError* error)
{
    il2cpp::vm::Thread::SetName((Il2CppInternalThread*)this_obj, (Il2CppString*)name);
    error_init(error);
}

void mono_thread_suspend_all_other_threads()
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

int32_t mono_thread_state_init_from_current(MonoThreadUnwindState* ctx)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return 0;
}

int32_t mono_thread_state_init_from_monoctx(MonoThreadUnwindState* ctx, MonoContext* mctx)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return 0;
}

const char* mono_property_get_name(MonoProperty *prop)
{
    return il2cpp::vm::Property::GetName((PropertyInfo*)prop);
}

MonoMethod* mono_property_get_get_method(MonoProperty *prop)
{
    return (MonoMethod*)il2cpp::vm::Property::GetGetMethod((PropertyInfo*)prop);
}

MonoMethod* mono_property_get_set_method(MonoProperty *prop)
{
    return (MonoMethod*)il2cpp::vm::Property::GetSetMethod((PropertyInfo*)prop);
}

MonoClass* mono_property_get_parent(MonoProperty *prop)
{
    return (MonoClass*)il2cpp::vm::Property::GetParent((PropertyInfo*)prop);
}

void mono_loader_lock()
{
#if IL2CPP_MONO_DEBUGGER
    il2cpp::utils::Debugger::AcquireLoaderLock();
#else
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
#endif
}

void mono_loader_unlock()
{
#if IL2CPP_MONO_DEBUGGER
    il2cpp::utils::Debugger::ReleaseLoaderLock();
#else
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
#endif
}

void mono_loader_lock_track_ownership(int32_t track)
{
    // This method intentionally does nothing.
}

int32_t mono_loader_lock_is_owned_by_self()
{
#if IL2CPP_MONO_DEBUGGER
    return il2cpp::utils::Debugger::LoaderLockIsOwnedByThisThread();
#else
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return false;
#endif
}

void mono_gc_wbarrier_generic_store(void* ptr, MonoObject* value)
{
    il2cpp::gc::WriteBarrier::GenericStore((Il2CppObject**)ptr, (Il2CppObject*)value);
}

void mono_gc_base_init()
{
    // This method intentionally does nothing.
}

#if IL2CPP_COMPILER_MSVC
int mono_gc_register_root(char* start, size_t size, MonoGCDescriptor descr, int32_t source, void* key, const char* msg)
#else
int mono_gc_register_root(char* start, size_t size, MonoGCDescriptor descr, MonoGCRootSource source, void* key, const char* msg)
#endif
{
    il2cpp::gc::GarbageCollector::RegisterRoot(start, size);
    return 1;
}

void mono_gc_deregister_root(char* addr)
{
    il2cpp::gc::GarbageCollector::UnregisterRoot(addr);
}

void* mono_gc_make_root_descr_all_refs(int numbits)
{
    return NULL;
}

#if IL2CPP_COMPILER_MSVC
int mono_gc_register_root_wbarrier(char *start, size_t size, MonoGCDescriptor descr, int32_t source, void *key, const char *msg)
#else
int mono_gc_register_root_wbarrier(char *start, size_t size, MonoGCDescriptor descr, MonoGCRootSource source, void *key, const char *msg)
#endif
{
    il2cpp::gc::GarbageCollector::RegisterRoot(start, size);
    return 1;
}

MonoGCDescriptor mono_gc_make_vector_descr()
{
    return 0;
}

MonoInterpCallbacks* mini_get_interp_callbacks()
{
#if IL2CPP_MONO_DEBUGGER
    return (MonoInterpCallbacks*)il2cpp::utils::Debugger::GetInterpCallbacks();
#else
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return NULL;
#endif
}

void* mono_gc_invoke_with_gc_lock(MonoGCLockedCallbackFunc func, void *data)
{
    return il2cpp::gc::GarbageCollector::CallWithAllocLockHeld(func, data);
}

int32_t mono_gc_is_moving()
{
    return false;
}

int mono_reflection_parse_type_checked(char *name, MonoTypeNameParse *monoInfo, MonoError *error)
{
#if !IL2CPP_MONO_DEBUGGER
    IL2CPP_ASSERT(0 && "This is not a complete implementation. It should only be called from the debugger.");
#endif
    error_init(error);
    il2cpp::vm::TypeNameParseInfo *pInfo = new il2cpp::vm::TypeNameParseInfo();
    std::string nameStr = name;
    std::replace(nameStr.begin(), nameStr.end(), '/', '+');
    il2cpp::vm::TypeNameParser parser(nameStr, *pInfo, false);
    Il2CppMonoTypeNameParse* info = (Il2CppMonoTypeNameParse*)monoInfo;
    info->assembly.name = NULL;
    info->il2cppTypeNameParseInfo = pInfo;
    return parser.Parse();
}

void mono_reflection_free_type_info(MonoTypeNameParse *info)
{
    delete (il2cpp::vm::TypeNameParseInfo*)((Il2CppMonoTypeNameParse*)info)->il2cppTypeNameParseInfo;
}

MonoType* mono_reflection_get_type_checked(MonoImage* rootimage, MonoImage* image, MonoTypeNameParse* info, int32_t ignorecase, int32_t* type_resolve, MonoError* error)
{
    error_init(error);

    Il2CppClass *klass = il2cpp::vm::Image::FromTypeNameParseInfo((Il2CppImage*)image, *((il2cpp::vm::TypeNameParseInfo*)((Il2CppMonoTypeNameParse*)info)->il2cppTypeNameParseInfo), ignorecase);
    if (!klass)
        return NULL;

    return (MonoType*)il2cpp::vm::Class::GetType(klass);
}

void mono_runtime_quit()
{
    il2cpp::vm::Runtime::Shutdown();
}

int32_t mono_runtime_is_shutting_down()
{
    return il2cpp::vm::Runtime::IsShuttingDown() ? true : false;
}

MonoObject* mono_runtime_try_invoke(MonoMethod* method, void* obj, void** params, MonoObject** exc, MonoError* error)
{
    error_init(error);

    return (MonoObject*)il2cpp::vm::Runtime::Invoke((MethodInfo*)method, obj, params, (Il2CppException**)exc);
}

MonoObject* mono_runtime_invoke_checked(MonoMethod* method, void* obj, void** params, MonoError* error)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return NULL;
}

int32_t mono_runtime_try_shutdown()
{
    return true;
}

void mono_arch_setup_resume_sighandler_ctx(MonoContext* ctx, void* func)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void mono_arch_set_breakpoint(MonoJitInfo* ji, uint8_t* ip)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void mono_arch_clear_breakpoint(MonoJitInfo* ji, uint8_t* ip)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void mono_arch_start_single_stepping()
{
}

void mono_arch_stop_single_stepping()
{
}

void mono_arch_skip_breakpoint(MonoContext* ctx, MonoJitInfo* ji)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void mono_arch_skip_single_step(MonoContext* ctx)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

intptr_t mono_arch_context_get_int_reg(MonoContext* ctx, int reg)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return 0;
}

void mono_arch_context_set_int_reg(MonoContext* ctx, int reg, intptr_t val)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

MonoJitInfo* mono_jit_info_table_find(MonoDomain* domain, char* addr)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return NULL;
}

MonoMethod* mono_jit_info_get_method(MonoJitInfo* ji)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return NULL;
}

MonoJitInfo* mono_jit_info_table_find_internal(MonoDomain* domain, char* addr, int32_t try_aot, int32_t allow_trampolines)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return NULL;
}

int32_t mono_debug_il_offset_from_address(MonoMethod* method, MonoDomain* domain, uint32_t native_offset)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return 0;
}

void mono_set_is_debugger_attached(int32_t attached)
{
#if IL2CPP_MONO_DEBUGGER
    il2cpp::utils::Debugger::SetIsDebuggerAttached(attached == 1);
#endif
}

uint32_t mono_aligned_addr_hash(const void* ptr)
{
    return ((uint32_t)(intptr_t)(ptr)) >> 3;
}

MonoGenericInst* mono_metadata_get_generic_inst(int type_argc, MonoType** type_argv)
{
    return (MonoGenericInst*)il2cpp::vm::MetadataCache::GetGenericInst((Il2CppType**)type_argv, type_argc);
}

void* mono_ldtoken_checked(MonoImage* image, uint32_t token, MonoClass** handle_class, MonoGenericContext* context, MonoError* error)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return 0;
}

void mono_stack_mark_record_size(MonoThreadInfo* info, HandleStackMark* stackmark, const char* func_name)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void mono_nullable_init(uint8_t* buf, MonoObject* value, MonoClass* klass)
{
    il2cpp::vm::Object::NullableInit(buf, (Il2CppObject*)value, (Il2CppClass*)klass);
}

MonoObject* mono_value_box_checked(MonoDomain* domain, MonoClass* klass, void* value, MonoError* error)
{
    error_init(error);
    return (MonoObject*)il2cpp::vm::Object::Box((Il2CppClass*)klass, value);
}

char* mono_get_runtime_build_info()
{
    return strdup("0.0 (IL2CPP)");
}

MonoMethod* mono_marshal_method_from_wrapper(MonoMethod* wrapper)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return NULL;
}

void* mono_jit_find_compiled_method_with_jit_info(MonoDomain* domain, MonoMethod* method, MonoJitInfo** ji)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return 0;
}

MonoLMF** mono_get_lmf_addr()
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return NULL;
}

void mono_set_lmf(MonoLMF* lmf)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void* il2cpp_mono_aot_get_method_checked(MonoDomain* domain, MonoMethod* method, MonoError* error)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return 0;
}

MonoJitInfo* mini_jit_info_table_find(MonoDomain* domain, char* addr, MonoDomain** out_domain)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return NULL;
}

void mono_restore_context(MonoContext* ctx)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

int32_t mono_error_ok(MonoError *error)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return 0;
}

MonoString* mono_ldstr_checked(MonoDomain* domain, MonoImage* image, uint32_t idx, MonoError* error)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return NULL;
}

int32_t mono_find_prev_seq_point_for_native_offset(MonoDomain *domain, MonoMethod *method, int32_t native_offset, MonoSeqPointInfo **info, SeqPoint* seq_point)
{
    IL2CPP_ASSERT(0 && "This method is not yet implemented");
    return 0;
}

int32_t mono_environment_exitcode_get()
{
    return il2cpp::vm::Runtime::GetExitCode();
}

void mono_environment_exitcode_set(int32_t value)
{
    il2cpp::vm::Runtime::SetExitCode(value);
}

void mono_threadpool_suspend()
{
    il2cpp::vm::ThreadPoolMs::Suspend();
}

void mono_threadpool_resume()
{
    il2cpp::vm::ThreadPoolMs::Resume();
}

MonoImage* mono_assembly_get_image(MonoAssembly* assembly)
{
    return (MonoImage*)il2cpp::vm::Assembly::GetImage((Il2CppAssembly*)assembly);
}

int32_t mono_verifier_is_method_valid_generic_instantiation(MonoMethod* method)
{
    if (!method)
        return 0;

    if (!((MethodInfo*)method)->is_generic && ((MethodInfo*)method)->is_inflated && ((MethodInfo*)method)->methodPointer)
        return 1;

    return 0;
}

void mono_network_init()
{
}

MonoMethod* jinfo_get_method(MonoJitInfo *ji)
{
    return (MonoMethod*)((Il2CppMonoJitInfo*)ji)->d.method;
}

void mono_error_cleanup(MonoError *oerror)
{
}

MonoGenericContext* mono_generic_class_get_context(MonoGenericClass *gclass)
{
    return (MonoGenericContext*)il2cpp::vm::GenericClass::GetContext((Il2CppGenericClass*)gclass);
}

MonoClass* mono_get_string_class()
{
    return (MonoClass*)il2cpp_defaults.string_class;
}
