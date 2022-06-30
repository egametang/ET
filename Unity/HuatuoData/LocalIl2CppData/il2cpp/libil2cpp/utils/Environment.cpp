#include "il2cpp-config.h"
#include "utils/StringUtils.h"
#include "utils/Environment.h"

namespace il2cpp
{
namespace utils
{
    static int s_ArgCount = 0;
    static std::vector<UTF16String> s_Args;

    void Environment::SetMainArgs(const char* const* args, int num_args)
    {
        s_ArgCount = num_args;
        s_Args.resize(num_args);

        for (int i = 0; i < num_args; i++)
            s_Args[i] = utils::StringUtils::Utf8ToUtf16(args[i]);
    }

    void Environment::SetMainArgs(const Il2CppChar* const* args, int num_args)
    {
        s_ArgCount = num_args;
        s_Args.resize(num_args);

        for (int i = 0; i < num_args; i++)
            s_Args[i] = args[i];
    }

    const std::vector<UTF16String>& Environment::GetMainArgs()
    {
        return s_Args;
    }

    int Environment::GetNumMainArgs()
    {
        return s_ArgCount;
    }
} /* namespace vm */
} /* namespace il2cpp */
