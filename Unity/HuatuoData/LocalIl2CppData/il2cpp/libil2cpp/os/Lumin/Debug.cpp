#include "il2cpp-config.h"

#if IL2CPP_TARGET_LUMIN

#include "os/Debug.h"
#include "utils/StringUtils.h"

#include "ml_logging.h"

#define TAG "IL2CPP"

namespace il2cpp
{
namespace os
{
    void Debug::WriteString(const utils::StringView<Il2CppNativeChar>& message)
    {
        ML_LOG_TAG(Debug, TAG, message.Str());
    }
}
}

#endif // IL2CPP_TARGET_LUMIN
