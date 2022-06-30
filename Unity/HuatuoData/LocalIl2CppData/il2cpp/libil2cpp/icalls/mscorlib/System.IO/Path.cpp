#include "il2cpp-config.h"

#include "icalls/mscorlib/System.IO/Path.h"
#include "vm/Path.h"
#include "vm/String.h"


namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace IO
{
    Il2CppString* Path::get_temp_path()
    {
        return il2cpp::vm::String::New(il2cpp::vm::Path::GetTempPath().c_str());
    }
} /* namespace IO */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
