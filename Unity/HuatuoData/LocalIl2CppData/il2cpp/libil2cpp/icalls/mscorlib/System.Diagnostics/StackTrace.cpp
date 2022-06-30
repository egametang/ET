#if !IL2CPP_TINY
#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
#include "gc/WriteBarrier.h"
#include "vm/Array.h"
#include "vm/Object.h"
#include "vm/Reflection.h"
#include "icalls/mscorlib/System.Diagnostics/StackTrace.h"
#include "vm-utils/DebugSymbolReader.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Diagnostics
{
    static Il2CppArray* GetTraceInternal(Il2CppArray* trace_ips, int32_t skip, bool need_file_info)
    {
        /* Exception is not thrown yet */
        if (trace_ips == NULL)
            return vm::Array::New(il2cpp_defaults.stack_frame_class, 0);

        int len = vm::Array::GetLength(trace_ips);
        Il2CppArray* stackFrames = vm::Array::New(il2cpp_defaults.stack_frame_class, len > skip ? len - skip : 0);

        for (int i = skip; i < len; i++)
        {
            Il2CppStackFrame* stackFrame = NULL;

            if (utils::DebugSymbolReader::DebugSymbolsAvailable())
            {
                stackFrame = il2cpp_array_get(trace_ips, Il2CppStackFrame*, i);
            }
            else
            {
                stackFrame = (Il2CppStackFrame*)vm::Object::New(il2cpp_defaults.stack_frame_class);
                MethodInfo* method = il2cpp_array_get(trace_ips, MethodInfo*, i);

                IL2CPP_OBJECT_SETREF(stackFrame, method, vm::Reflection::GetMethodObject(method, NULL));
            }

            il2cpp_array_setref(stackFrames, i, stackFrame);
        }

        return stackFrames;
    }

    Il2CppArray* StackTrace::get_trace(Il2CppException *exc, int32_t skip, bool need_file_info)
    {
        // Exception.RestoreExceptionDispatchInfo() will clear trace_ips, so we need to ensure that we read it only once
        return GetTraceInternal(exc->trace_ips, skip, need_file_info);
    }
} /* namespace Diagnostics */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
#endif
