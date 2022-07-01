// DO NOT PUT #pragma once or include guard check here
// This header is designed to be able to be included multiple times

// As external code and 3rd party headers can collide with our platform defines or compiler emulation,
// this header can be used to disable them. The corresponding 'PostExternalInclude.h' enables the emulation
// and platform defines again.
//
// Usage:
// #include "PreExternalInclude.h"
// #include "SomeExternalCode.h"
// #include "3rdParty.h"
// #include "PostExternalInclude.h"

#if DETAIL__PREEXTERNALINCLUDE_HAS_BEEN_INCLUDED
    #error "PreExternalInclude.h has been included more than once or PostExternalInclude.h is missing."
#endif

#include "Internal/UndefineCompilerMacros.h"
#include "Internal/UndefinePlatforms.h"
#include "Internal/UndefineCoreMacros.h"

#define DETAIL__PREEXTERNALINCLUDE_HAS_BEEN_INCLUDED

// detect whether windows SDK winuser.h has been included before, to
// optionally restore some state afterwards (some 3rd party libraries
// include windows.h which defines a lot of names as macros)
#if defined(_WINUSER_)
#   define DETAIL__WINUSER_H_HAS_BEEN_INCLUDED 1
#else
#   define DETAIL__WINUSER_H_HAS_BEEN_INCLUDED 0
#endif
