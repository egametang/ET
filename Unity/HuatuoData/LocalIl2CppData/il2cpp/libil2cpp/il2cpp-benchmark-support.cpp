#if IL2CPP_GOOGLE_BENCHMARK

#include "il2cpp-config.h"
#include "utils/StringUtils.h"
#include <benchmark/benchmark.h>

void il2cpp_benchmark_initialize(int argc, const Il2CppChar* const* argv)
{
    std::vector<std::string> args(argc);
    for (int i = 0; i < argc; ++i)
        args[i] = il2cpp::utils::StringUtils::Utf16ToUtf8(argv[i], -1);

    std::vector<const char*> cargs(argc);
    for (int i = 0; i < argc; ++i)
        cargs[i] = args[i].c_str();

    benchmark::Initialize(&argc, const_cast<char**>(&cargs[0]));
}

void il2cpp_benchmark_initialize(int argc, const char* const* argv)
{
    benchmark::Initialize(&argc, const_cast<char**>(argv));
}

#endif
