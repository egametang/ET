#include "il2cpp-config.h"
#include "Finally.h"
#include "vm/Exception.h"

NORETURN void il2cpp::utils::RethrowException(Il2CppException* exception)
{
#if !RUNTIME_TINY
    vm::Exception::Rethrow(exception);
#else
    tiny::vm::Exception::Raise(exception);
#endif
}
