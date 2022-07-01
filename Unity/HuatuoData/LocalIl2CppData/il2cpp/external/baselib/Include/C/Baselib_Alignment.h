#pragma once

// C99 versions of align_of, align_as
#ifndef BASELIB_ALIGN_OF
    #define BASELIB_ALIGN_OF(TYPE_) COMPILER_ALIGN_OF(TYPE_)
#endif

#ifndef BASELIB_ALIGN_AS
    #define BASELIB_ALIGN_AS(ALIGNMENT_) COMPILER_ALIGN_AS(ALIGNMENT_)
#endif
