#pragma once

namespace il2cpp
{
namespace utils
{
    class NonCopyable
    {
    public:
        NonCopyable() {}

    private:
        NonCopyable(const NonCopyable&);
        NonCopyable& operator=(const NonCopyable&);
    };
}
}
