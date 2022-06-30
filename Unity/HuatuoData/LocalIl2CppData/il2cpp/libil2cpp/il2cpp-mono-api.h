#pragma once

// This header defines the Mono embedding API that the debugger code requires.
// It should not include any Mono headers.

#include <stdint.h>

#include "il2cpp-config-api.h"
#include "il2cpp-class-internals.h"
#include "vm-utils/Debugger.h"
#include "vm/GlobalMetadataFileInternals.h"

typedef struct _MonoAppDomain MonoAppDomain;
typedef struct _MonoArray MonoArray;
typedef struct _MonoAssembly MonoAssembly;
typedef struct _MonoClass MonoClass;
typedef struct _MonoClassField MonoClassField;
typedef struct _MonoDebugLocalsInfo MonoDebugLocalsInfo;
typedef struct _MonoDebugMethodJitInfo MonoDebugMethodJitInfo;
typedef struct _MonoDomain MonoDomain;
typedef struct _MonoError MonoError;
typedef struct _MonoException MonoException;
typedef struct _MonoGenericClass MonoGenericClass;
typedef struct _MonoGenericContainer MonoGenericContainer;
typedef struct _MonoGenericContext MonoGenericContext;
typedef struct _MonoGenericInst MonoGenericInst;
typedef struct _MonoGenericParam MonoGenericParam;
typedef struct _MonoImage MonoImage;
typedef struct _MonoInternalThread MonoInternalThread;
typedef struct _MonoMethod MonoMethod;
typedef struct _MonoMethodInflated MonoMethodInflated;
typedef struct _MonoMethodHeader MonoMethodHeader;
typedef struct _MonoMethodSignature MonoMethodSignature;
typedef struct _MonoObject MonoObject;
typedef struct _MonoProperty MonoProperty;
typedef struct _MonoReflectionType MonoReflectionType;
typedef struct _MonoString MonoString;
typedef struct _MonoThread MonoThread;
typedef struct _MonoType MonoType;
typedef struct MonoVTable MonoVTable;
typedef struct MonoTypeNameParse MonoTypeNameParse;
typedef struct _GPtrArray GPtrArray;
typedef void* MonoGCDescriptor;

struct MonoThreadInfo;

typedef void (*MonoDomainFunc) (MonoDomain *domain, void* user_data);
typedef int32_t (*Il2CppMonoInternalStackWalk) (void /*MonoStackFrameInfo*/ *frame, void /*MonoContext*/ *ctx, void* data);
typedef void* (*MonoGCLockedCallbackFunc) (void *data);

typedef struct
{
    void* (*create_method_pointer)(MethodInfo * method, void /*MonoError*/ *error);
    Il2CppObject* (*runtime_invoke)(MethodInfo * method, void *obj, void **params, Il2CppObject **exc, void /*MonoError*/  *error);
    void (*init_delegate)(Il2CppDelegate *del);
#ifndef DISABLE_REMOTING
    void* (*get_remoting_invoke)(void* imethod, void /*MonoError*/  *error);
#endif
    void* (*create_trampoline)(Il2CppDomain * domain, MethodInfo *method, void /*MonoError*/  *error);
    void (*walk_stack_with_ctx)(Il2CppMonoInternalStackWalk func, void /*MonoContext*/  *ctx, int32_t /*MonoUnwindOptions*/ options, void *user_data);
    void (*set_resume_state)(void /*MonoJitTlsData*/ *jit_tls, Il2CppException *ex, void /*MonoJitExceptionInfo*/ *ei, void /*Il2CppSequencePointExecutionContext*/* interp_frame, void* handler_ip);
    int32_t (*run_finally)(void /*MonoStackFrameInfo*/  *frame, int clause_index, void* handler_ip);
    int32_t (*run_filter)(void /*MonoStackFrameInfo*/  *frame, Il2CppException *ex, int clause_index, void* handler_ip);
    void (*frame_iter_init)(void /*MonoInterpStackIter*/ *iter, void* interp_exit_data);
    int32_t (*frame_iter_next)(void /*MonoInterpStackIter*/ *iter, void /*MonoStackFrameInfo*/  *frame);
    void /*MonoJitInfo*/* (*find_jit_info)(Il2CppDomain * domain, MethodInfo *method);
    void (*set_breakpoint)(void /*MonoJitInfo*/ *jinfo, void* ip);
    void (*clear_breakpoint)(void /*MonoJitInfo*/ *jinfo, void* ip);
    void /*MonoJitInfo*/* (*frame_get_jit_info)(void /*Il2CppSequencePointExecutionContext*/* frame);
    void* (*frame_get_ip)(void /*Il2CppSequencePointExecutionContext*/* frame);
    void* (*frame_get_arg)(void /*Il2CppSequencePointExecutionContext*/* frame, int pos);
    void* (*frame_get_local)(void /*Il2CppSequencePointExecutionContext*/* frame, int pos);
    void* (*frame_get_this)(void /*Il2CppSequencePointExecutionContext*/* frame);
    void /*Il2CppSequencePointExecutionContext*/* (*frame_get_parent)(void /*Il2CppSequencePointExecutionContext*/* frame);
    void (*start_single_stepping)();
    void (*stop_single_stepping)();
} MonoInterpCallbacks;

#if defined(__cplusplus)
enum class MonoTypeNameFormat;
#else
enum MonoTypeNameFormat;
#endif // __cplusplus

#if defined(__cplusplus)
extern "C"
{
#endif // __cplusplus
#define DO_MONO_API(r, n, p)             IL2CPP_EXPORT r n p;
#define DO_MONO_API_NO_RETURN(r, n, p)   IL2CPP_EXPORT NORETURN r n p;
#define DO_MONO_API_NOT_EXPORTED(r, n, p) r n p;
#include "il2cpp-mono-api-functions.h"
#undef DO_MONO_API
#undef DO_API_NORETURN
#if defined(__cplusplus)
}
#endif // __cplusplus
