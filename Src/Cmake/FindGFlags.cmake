function(_gflags_append_debugs _endvar _library)
    if(${_library} AND ${_library}_DEBUG)
        set(_output optimized ${${_library}} debug ${${_library}_DEBUG})
    else()
        set(_output ${${_library}})
    endif()
    set(${_endvar} ${_output} PARENT_SCOPE)
endfunction()

function(_gflags_find_library _name)
    find_library(${_name}
        NAMES ${ARGN}
        HINTS
            $ENV{GFLAGS_ROOT}
            ${GFLAGS_ROOT}
        PATH_SUFFIXES ${_gflags_libpath_suffixes}
    )
    mark_as_advanced(${_name})
endfunction()

if(NOT DEFINED GFLAGS_MSVC_SEARCH)
    set(GFLAGS_MSVC_SEARCH MD)
endif()

set(_gflags_libpath_suffixes lib)
if(MSVC)
    if(GFLAGS_MSVC_SEARCH STREQUAL "MD")
        list(APPEND _gflags_libpath_suffixes
            msvc/gflags-md/Debug
            msvc/gflags-md/Release)
    elseif(GFLAGS_MSVC_SEARCH STREQUAL "MT")
        list(APPEND _gflags_libpath_suffixes
            msvc/gflags/Debug
            msvc/gflags/Release)
    endif()
endif()


find_path(GFLAGS_INCLUDE_DIR gflags/gflags.h
    HINTS
        $ENV{GFLAGS_ROOT}/include
        ${GFLAGS_ROOT}/include
)
mark_as_advanced(GFLAGS_INCLUDE_DIR)

if(MSVC AND GFLAGS_MSVC_SEARCH STREQUAL "MD")
    # The provided /MD project files for Google Log add -md suffixes to the
    # library names.
    _gflags_find_library(GFLAGS_LIBRARY            gflags-md  gflags)
else()
    _gflags_find_library(GFLAGS_LIBRARY            gflags)
endif()

INCLUDE(FindPackageHandleStandardArgs)
FIND_PACKAGE_HANDLE_STANDARD_ARGS(GFlags DEFAULT_MSG GFLAGS_LIBRARY)

if(GFLAGS_FOUND)
    set(GFLAGS_INCLUDE_DIRS ${GFLAGS_INCLUDE_DIR})
    _gflags_append_debugs(GFLAGS_LIBRARIES      GFLAGS_LIBRARY)
endif()
