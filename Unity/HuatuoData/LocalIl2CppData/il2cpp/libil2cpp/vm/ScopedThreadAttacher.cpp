#include "il2cpp-config.h"
#include "vm/Thread.h"
#include "vm/Domain.h"
#include "vm/Runtime.h"
#include "ScopedThreadAttacher.h"

il2cpp::vm::ScopedThreadAttacher::ScopedThreadAttacher()
    : m_AttachedThread(NULL)
{
#if IL2CPP_TINY
    if (il2cpp::vm::Thread::Current() != il2cpp::vm::Thread::Main())
        il2cpp::vm::Runtime::FailFast("Managed code must be compiled with Burst to execute on a non-main thread.");
#else
    if (il2cpp::vm::Thread::Current() == NULL)
        m_AttachedThread = il2cpp::vm::Thread::Attach(il2cpp::vm::Domain::GetRoot());
#endif
}

il2cpp::vm::ScopedThreadAttacher::~ScopedThreadAttacher()
{
#if !IL2CPP_TINY
    if (m_AttachedThread != NULL)
        il2cpp::vm::Thread::Detach(m_AttachedThread);
#endif
}
