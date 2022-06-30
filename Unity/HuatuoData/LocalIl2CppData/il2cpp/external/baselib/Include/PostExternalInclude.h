// DO NOT PUT #pragma once or include guard check here
// This header is designed to be able to be included multiple times

// This header should only be used together with PreExternalInclude.h.
// See PreExternalInclude.h for usage information.

#ifndef DETAIL__PREEXTERNALINCLUDE_HAS_BEEN_INCLUDED
    #error "PostExternalInclude.h can only be included after PreExternalInclude.h got included before."
#endif

#undef DETAIL__PREEXTERNALINCLUDE_HAS_BEEN_INCLUDED

// if 3rd party library made windows.h be included, undefine SendMessage
// macro that winuser.h declared
#if !DETAIL__WINUSER_H_HAS_BEEN_INCLUDED && defined(_WINUSER_)
#undef SendMessage
#endif
#undef DETAIL__WINUSER_H_HAS_BEEN_INCLUDED

#include "Internal/RedefineCompilerMacros.h"

// undefine whatever might be defined already
#include "Internal/UndefineCoreMacros.h"
#include "Internal/CoreMacros.h"
