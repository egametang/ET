#include "il2cpp-config.h"
#include "vm/Thread.h"
#include "vm/Domain.h"
#include "ScopedThreadAttacher.h"

il2cpp::vm::ScopedThreadAttacher::ScopedThreadAttacher()
    : m_AttachedThread(NULL)
{
    if (il2cpp::vm::Thread::Current() == NULL)
        m_AttachedThread = il2cpp::vm::Thread::Attach(il2cpp::vm::Domain::GetRoot());
}

il2cpp::vm::ScopedThreadAttacher::~ScopedThreadAttacher()
{
    if (m_AttachedThread != NULL)
        il2cpp::vm::Thread::Detach(m_AttachedThread);
}
