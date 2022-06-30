#include "il2cpp-config.h"
#include "icalls/mscorlib/System/Type.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
#include "vm/Type.h"
#include <utils/StringUtils.h>
#include <metadata/GenericMetadata.h>

using il2cpp::metadata::GenericMetadata;
using il2cpp::utils::StringUtils;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    Il2CppReflectionType* Type::internal_from_handle(intptr_t ptr)
    {
        return vm::Type::GetTypeFromHandle(ptr);
    }
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
