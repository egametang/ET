#pragma once

#include "il2cpp-config.h"

#if IL2CPP_TARGET_JAVASCRIPT

#define IL2CPP_USES_POSIX_CLASS_LIBRARY_PAL 1

#define IL2CPP_HAVE_STAT_FLAGS 1
#define IL2CPP_HAVE_STAT_FLAGS 1
#define IL2CPP_HAVE_LCHFLAGS 1
#define IL2CPP_HAVE_FUTIMENS 1
#define IL2CPP_HAVE_SYS_UN 1

#define stat_ stat
#define fstat_ fstat
#define lstat_ lstat

#include <dirent.h>

#endif // IL2CPP_TARGET_JAVASCRIPT
