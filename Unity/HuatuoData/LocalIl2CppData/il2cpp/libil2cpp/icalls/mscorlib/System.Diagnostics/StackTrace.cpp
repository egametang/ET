#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
#include "vm/Array.h"
#include "vm/Object.h"
#include "vm/Reflection.h"
#include "icalls/mscorlib/System.Diagnostics/StackTrace.h"

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
    Il2CppArray* StackTrace::get_trace(Il2CppException *exc, int32_t skip, bool need_file_info)
    {
        Il2CppArray *res;
        Il2CppArray *ta = exc->trace_ips;
        int i, len;

        if (ta == NULL)
        {
            /* Exception is not thrown yet */
            return vm::Array::New(il2cpp_defaults.stack_frame_class, 0);
        }

        len = vm::Array::GetLength(ta);

        res = vm::Array::New(il2cpp_defaults.stack_frame_class, len > skip ? len - skip : 0);

        for (i = skip; i < len; i++)
        {
            Il2CppStackFrame *sf = (Il2CppStackFrame*)vm::Object::New(il2cpp_defaults.stack_frame_class);
            MethodInfo* method = il2cpp_array_get(ta, MethodInfo*, i);

            IL2CPP_OBJECT_SETREF(sf, method, vm::Reflection::GetMethodObject(method, NULL));

            il2cpp_array_setref(res, i, sf);
        }

        return res;
    }
} /* namespace Diagnostics */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
