#include "il2cpp-config.h"

#include "gc/WriteBarrier.h"
#include "icalls/mscorlib/System/ConsoleDriver.h"
#include "il2cpp-class-internals.h"
#include "os/Console.h"
#include "os/File.h"
#include "vm/Array.h"
#include "vm/Exception.h"
#include "utils/StringUtils.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
// Used in .NET 2.0 System.Console -> CStreamReader/CStreamWriter -> TermInfoDriver -> ConsoleDriver -> icalls

    bool ConsoleDriver::Isatty(intptr_t handle)
    {
        il2cpp::os::FileHandle* fileHandle = (il2cpp::os::FileHandle*)handle;
        auto result = os::File::Isatty(fileHandle);
        vm::Exception::RaiseIfError(result.GetError());
        return result.Get();
    }

    int32_t ConsoleDriver::InternalKeyAvailable(int32_t ms_timeout)
    {
        return il2cpp::os::Console::InternalKeyAvailable(ms_timeout);
    }

    bool ConsoleDriver::TtySetup(Il2CppString* keypadXmit, Il2CppString* teardown, Il2CppArray** control_characters, int32_t** size)
    {
        const std::string keypadXmitString(keypadXmit ? il2cpp::utils::StringUtils::Utf16ToUtf8(keypadXmit->chars) : "");
        const std::string teardownString(teardown ? il2cpp::utils::StringUtils::Utf16ToUtf8(teardown->chars) : "");

        uint8_t controlChars[17];

        const bool ret = il2cpp::os::Console::TtySetup(keypadXmitString, teardownString, controlChars, size);

        gc::WriteBarrier::GenericStore(control_characters, vm::Array::New(il2cpp_defaults.byte_class, 17));

        if (ret)
            memcpy(il2cpp_array_addr(*control_characters, uint8_t, 0), controlChars, 17);

        return true;
    }

    bool ConsoleDriver::SetEcho(bool wantEcho)
    {
        return il2cpp::os::Console::SetEcho(wantEcho);
    }

    bool ConsoleDriver::SetBreak(bool wantBreak)
    {
        return il2cpp::os::Console::SetBreak(wantBreak);
    }
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
