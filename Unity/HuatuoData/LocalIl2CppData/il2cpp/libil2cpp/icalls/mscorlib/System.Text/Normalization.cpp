#include "il2cpp-config.h"
#include "Normalization.h"
#include <il2cpp-normalization-tables.h>

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Text
{
    void Normalization::load_normalization_resource(intptr_t* argProps, intptr_t* argMappedChars, intptr_t* argCharMapIndex, intptr_t* argHelperIndex, intptr_t* argMapIdxToComposite, intptr_t* argCombiningClass)
    {
        *argProps = reinterpret_cast<intptr_t>(props);
        *argMappedChars = reinterpret_cast<intptr_t>(mappedChars);
        *argCharMapIndex = reinterpret_cast<intptr_t>(charMapIndex);
        *argHelperIndex = reinterpret_cast<intptr_t>(helperIndex);
        *argMapIdxToComposite = reinterpret_cast<intptr_t>(mapIdxToComposite);
        *argCombiningClass = reinterpret_cast<intptr_t>(combiningClass);
    }
} // namespace Text
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
