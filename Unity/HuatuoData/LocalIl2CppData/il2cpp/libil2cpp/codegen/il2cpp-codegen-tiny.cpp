#include "il2cpp-config.h"

#if RUNTIME_TINY

#include "il2cpp-codegen.h"

#include "vm/StackTrace.h"
#include "vm/LastError.h"

#include <string>

void il2cpp_codegen_stacktrace_push_frame(TinyStackFrameInfo& frame)
{
    tiny::vm::StackTrace::PushFrame(frame);
}

void il2cpp_codegen_stacktrace_pop_frame()
{
    tiny::vm::StackTrace::PopFrame();
}

void il2cpp_codegen_marshal_store_last_error()
{
    tiny::vm::LastError::StoreLastError();
}

NORETURN void il2cpp_codegen_raise_generic_virtual_method_exception(const char* methodFullName)
{
    std::string message;
    message = "Tiny does not support generic virtual method invocation. ";
    message += "The method being invoked is: '";
    message += methodFullName;
    message += "'";
    il2cpp_codegen_raise_exception(message.c_str());
    IL2CPP_UNREACHABLE;
}

#endif
