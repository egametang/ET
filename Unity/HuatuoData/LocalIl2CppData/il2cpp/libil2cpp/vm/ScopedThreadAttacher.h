#pragma once

struct Il2CppThread;

namespace il2cpp
{
namespace vm
{
    class ScopedThreadAttacher
    {
    public:
        ScopedThreadAttacher();
        ~ScopedThreadAttacher();

    private:
        Il2CppThread* m_AttachedThread;
    };
}
}
