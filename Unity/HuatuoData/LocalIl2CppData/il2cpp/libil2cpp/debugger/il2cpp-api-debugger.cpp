#if defined(RUNTIME_IL2CPP) && !defined(IL2CPP_MONO_DEBUGGER_DISABLED)

// This file implements an extension to the IL2CPP embedding API that the debugger code requires.
// It should not include any Mono headers.

#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-mono-api.h"
#include "il2cpp-api-debugger.h"

#include "metadata/FieldLayout.h"
#include "metadata/GenericMetadata.h"
#include "vm/Assembly.h"
#include "vm/AssemblyName.h"
#include "vm/ClassInlines.h"
#include "vm/GenericClass.h"
#include "vm/GenericContainer.h"
#include "vm/Image.h"
#include "vm/MetadataCache.h"
#include "vm/Reflection.h"
#include "vm-utils/Debugger.h"

#if IL2CPP_TARGET_XBOXONE
#define strdup _strdup
#endif

extern "C" {
    Il2CppDefaults il2cpp_mono_defaults;

    void* il2cpp_domain_get_agent_info(MonoAppDomain* domain)
    {
        return ((Il2CppDomain*)domain)->agent_info;
    }

    void il2cpp_domain_set_agent_info(MonoAppDomain* domain, void* agentInfo)
    {
        ((Il2CppDomain*)domain)->agent_info = agentInfo;
    }

    void il2cpp_start_debugger_thread()
    {
#if IL2CPP_MONO_DEBUGGER
        il2cpp::utils::Debugger::StartDebuggerThread();
#endif
    }

    const char* il2cpp_domain_get_name(MonoDomain* domain)
    {
        return ((Il2CppDomain*)domain)->friendly_name;
    }

    Il2CppSequencePoint* il2cpp_get_method_sequence_points(MonoMethod* method, void* *iter)
    {
#if IL2CPP_MONO_DEBUGGER
        if (method == NULL)
            return il2cpp::utils::Debugger::GetAllSequencePoints(iter);
        else
            return (Il2CppSequencePoint*)il2cpp::utils::Debugger::GetSequencePoints((const MethodInfo*)method, iter);
#else
        return NULL;
#endif
    }

    Il2CppCatchPoint* il2cpp_get_method_catch_points(MonoMethod* method, void* *iter)
    {
#if IL2CPP_MONO_DEBUGGER
        return (Il2CppCatchPoint*)il2cpp::utils::Debugger::GetCatchPoints((const MethodInfo*)method, iter);
#else
        return NULL;
#endif
    }

    Il2CppSequencePoint* il2cpp_get_seq_point_from_catch_point(Il2CppCatchPoint *cp)
    {
#if IL2CPP_MONO_DEBUGGER
        return (Il2CppSequencePoint*)il2cpp::utils::Debugger::GetSequencePoint(NULL, cp);
#else
        return NULL;
#endif
    }

    int32_t il2cpp_mono_methods_match(MonoMethod* left, MonoMethod* right)
    {
        MethodInfo* leftMethod = (MethodInfo*)left;
        MethodInfo* rightMethod = (MethodInfo*)right;

        if (rightMethod == leftMethod)
            return 1;
        if (rightMethod == NULL || leftMethod == NULL)
            return 0;
        if (leftMethod->methodMetadataHandle == rightMethod->methodMetadataHandle)
            return 1;

        return 0;
    }

    MonoClass* il2cpp_defaults_object_class()
    {
        return (MonoClass*)il2cpp_defaults.object_class;
    }

    const char* il2cpp_image_name(MonoImage *monoImage)
    {
        Il2CppImage *image = (Il2CppImage*)monoImage;
        return image->name;
    }

    uint8_t* il2cpp_field_get_address(MonoObject *obj, MonoClassField *monoField)
    {
        FieldInfo *field = (FieldInfo*)monoField;
        return (uint8_t*)obj + field->offset;
    }

    MonoClass* il2cpp_defaults_exception_class()
    {
        return (MonoClass*)il2cpp_defaults.exception_class;
    }

    MonoImage* il2cpp_defaults_corlib_image()
    {
        return (MonoImage*)il2cpp_defaults.corlib;
    }

    bool il2cpp_method_is_string_ctor(const MonoMethod * method)
    {
        MethodInfo* methodInfo = (MethodInfo*)method;
        return methodInfo->klass == il2cpp_defaults.string_class && !strcmp(methodInfo->name, ".ctor");
    }

    MonoClass* il2cpp_defaults_void_class()
    {
        return (MonoClass*)il2cpp_defaults.void_class;
    }

    MonoMethod* il2cpp_get_interface_method(MonoClass* klass, MonoClass* itf, int slot)
    {
        const VirtualInvokeData* data = il2cpp::vm::ClassInlines::GetInterfaceInvokeDataFromVTable((Il2CppClass*)klass, (Il2CppClass*)itf, slot);
        if (!data)
            return NULL;

        return (MonoMethod*)data->method;
    }

    struct TypeIterState
    {
        il2cpp::vm::AssemblyVector* assemblies;
        il2cpp::vm::AssemblyVector::iterator assembly;
        Il2CppImage* image;
        il2cpp::vm::TypeVector types;
        il2cpp::vm::TypeVector::iterator type;
    };

    MonoClass* il2cpp_iterate_loaded_classes(void* *iter)
    {
        if (!iter)
            return NULL;

        if (!*iter)
        {
            TypeIterState *state = new TypeIterState();
            state->assemblies = il2cpp::vm::Assembly::GetAllAssemblies();
            state->assembly = state->assemblies->begin();
            state->image = il2cpp::vm::Assembly::GetImage(*state->assembly);
            il2cpp::vm::Image::GetTypes(state->image, true, &state->types);
            state->type = state->types.begin();
            *iter = state;
            return (MonoClass*)*state->type;
        }

        TypeIterState *state = (TypeIterState*)*iter;

        state->type++;
        if (state->type == state->types.end())
        {
            state->assembly++;
            if (state->assembly == state->assemblies->end())
            {
                delete state;
                *iter = NULL;
                return NULL;
            }

            state->image = il2cpp::vm::Assembly::GetImage(*state->assembly);
            il2cpp::vm::Image::GetTypes(state->image, true, &state->types);
            state->type = state->types.begin();
        }

        return (MonoClass*)*state->type;
    }

    const char** il2cpp_get_source_files_for_type(MonoClass *klass, int *count)
    {
#if IL2CPP_MONO_DEBUGGER
        return il2cpp::utils::Debugger::GetTypeSourceFiles((Il2CppClass*)klass, *count);
#else
        return NULL;
#endif
    }

    MonoMethod* il2cpp_method_get_generic_definition(MonoMethodInflated *imethod)
    {
        MethodInfo *method = (MethodInfo*)imethod;

        if (!method->is_inflated || method->is_generic)
            return NULL;

        return (MonoMethod*)((MethodInfo*)imethod)->genericMethod->methodDefinition;
    }

    MonoGenericInst* il2cpp_method_get_generic_class_inst(MonoMethodInflated *imethod)
    {
        MethodInfo *method = (MethodInfo*)imethod;

        if (!method->is_inflated || method->is_generic)
            return NULL;

        return (MonoGenericInst*)method->genericMethod->context.class_inst;
    }

    MonoClass* il2cpp_generic_class_get_container_class(MonoGenericClass *gclass)
    {
        return (MonoClass*)il2cpp::vm::GenericClass::GetTypeDefinition((Il2CppGenericClass*)gclass);
    }

    Il2CppSequencePoint* il2cpp_get_sequence_point(MonoImage* image, int id)
    {
#if IL2CPP_MONO_DEBUGGER
        return il2cpp::utils::Debugger::GetSequencePoint((const Il2CppImage*)image, id);
#else
        return NULL;
#endif
    }

    char* il2cpp_assembly_get_full_name(MonoAssembly *assembly)
    {
        std::string s = il2cpp::vm::AssemblyName::AssemblyNameToString(((Il2CppAssembly*)assembly)->aname);
        return strdup(s.c_str());
    }

    const MonoMethod* il2cpp_get_seq_point_method(Il2CppSequencePoint *seqPoint)
    {
#if IL2CPP_MONO_DEBUGGER
        return (const MonoMethod*)il2cpp::utils::Debugger::GetSequencePointMethod(NULL, seqPoint);
#else
        return NULL;
#endif
    }

    const MonoClass* il2cpp_get_class_from_index(int index)
    {
        if (index < 0)
            return NULL;

        return (const MonoClass*)il2cpp::vm::MetadataCache::GetTypeInfoFromTypeIndex(NULL, index);
    }

    const MonoType* il2cpp_type_inflate(MonoType* type, const MonoGenericContext* context)
    {
        return (MonoType*)il2cpp::metadata::GenericMetadata::InflateIfNeeded((Il2CppType*)type, (const Il2CppGenericContext*)context, true);
    }

    void il2cpp_debugger_get_method_execution_context_and_header_info(const MonoMethod* method, uint32_t* executionContextInfoCount, const Il2CppMethodExecutionContextInfo **executionContextInfo, const Il2CppMethodHeaderInfo **headerInfo, const Il2CppMethodScope **scopes)
    {
#if IL2CPP_MONO_DEBUGGER
        il2cpp::utils::Debugger::GetMethodExecutionContextInfo((const MethodInfo*)method, executionContextInfoCount, executionContextInfo, headerInfo, scopes);
#endif
    }

    Il2CppThreadUnwindState* il2cpp_debugger_get_thread_context()
    {
#if IL2CPP_MONO_DEBUGGER
        return il2cpp::utils::Debugger::GetThreadStatePointer();
#else
        return NULL;
#endif
    }

    Il2CppSequencePointSourceFile* il2cpp_debug_get_source_file(MonoImage* image, int index)
    {
        return ((Il2CppImage*)image)->codeGenModule->debuggerMetadata->sequencePointSourceFiles + index;
    }

    size_t il2cpp_type_size(MonoType *t)
    {
        return il2cpp::metadata::FieldLayout::GetTypeSizeAndAlignment((Il2CppType*)t).size;
    }

    MonoMethod* il2cpp_get_generic_method_definition(MonoMethod* method)
    {
        return (MonoMethod*)((MethodInfo*)method)->genericMethod->methodDefinition;
    }

    bool il2cpp_class_is_initialized(MonoClass* klass)
    {
        return ((Il2CppClass*)klass)->initialized;
    }

    int il2cpp_generic_inst_get_argc(MonoGenericInst* inst)
    {
        return ((Il2CppGenericInst*)inst)->type_argc;
    }

    MonoType* il2cpp_generic_inst_get_argv(MonoGenericInst* inst, int index)
    {
        return (MonoType*)((Il2CppGenericInst*)inst)->type_argv[index];
    }

    MonoObject* il2cpp_assembly_get_object(MonoDomain* domain, MonoAssembly* assembly, MonoError* error)
    {
        return (MonoObject*)il2cpp::vm::Reflection::GetAssemblyObject((const Il2CppAssembly *)assembly);
    }

    const MonoType* il2cpp_get_type_from_index(int index)
    {
        return (const MonoType*)il2cpp::vm::MetadataCache::GetIl2CppTypeFromIndex(NULL, index);
    }

    void il2cpp_thread_info_safe_suspend_and_run(size_t /*Really MonoNativeThreadId*/ id, int32_t interrupt_kernel, MonoSuspendThreadCallback callback, void* user_data)
    {
        callback(NULL, user_data);
    }

    MonoGenericParam* il2cpp_generic_container_get_param(MonoGenericContainer *gc, int i)
    {
        return (MonoGenericParam*)il2cpp::vm::GenericContainer::GetGenericParameter((Il2CppMetadataGenericContainerHandle)gc, i);
    }
}
#endif // RUNTIME_IL2CPP
