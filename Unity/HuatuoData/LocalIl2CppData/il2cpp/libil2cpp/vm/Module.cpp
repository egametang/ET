#include "vm/Module.h"
#include "il2cpp-class-internals.h"

namespace il2cpp
{
namespace vm
{
    uint32_t Module::GetToken(const Il2CppImage *image)
    {
        return image->token;
    }
} /* namespace vm */
} /* namespace il2cpp */
