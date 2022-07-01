#include "il2cpp-config.h"
#include "il2cpp-vm-support.h"

#if IL2CPP_TARGET_LUMIN
#include "il2cpp-class-internals.h"
#include "os/Environment.h"
#include "il2cpp-api.h"

#include <sys/system_properties.h>

namespace il2cpp
{
namespace os
{
    std::string Environment::GetMachineName()
    {
        char buf[256] = {0};
        std::string name = "lumin";

        if (!__system_property_get("ro.serialno", buf))
            return name;

        return name + "_" + buf;
    }
}
}
#endif //IL2CPP_TARGET_LUMIN
