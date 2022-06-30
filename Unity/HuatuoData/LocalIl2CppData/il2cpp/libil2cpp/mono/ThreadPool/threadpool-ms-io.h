#pragma once

#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"

struct Il2CppIOSelectorJob;

void threadpool_ms_io_remove_socket(int fd);
//void mono_threadpool_ms_io_remove_domain_jobs (MonoDomain *domain);
void threadpool_ms_io_cleanup(void);

LIBIL2CPP_CODEGEN_API void ves_icall_System_IOSelector_Add(intptr_t handle, Il2CppIOSelectorJob *job);
LIBIL2CPP_CODEGEN_API void ves_icall_System_IOSelector_Remove(intptr_t handle);
