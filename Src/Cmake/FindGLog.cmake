function(_glog_append_debugs _endvar _library)
    if(${_library} AND ${_library}_DEBUG)
        set(_output optimized ${${_library}} debug ${${_library}_DEBUG})
    else()
        set(_output ${${_library}})
    endif()
    set(${_endvar} ${_output} PARENT_SCOPE)
endfunction()

function(_glog_find_library _name)
    find_library(${_name}
        NAMES ${ARGN}
        HINTS
            $ENV{GLOG_ROOT}
            ${GLOG_ROOT}
        PATH_SUFFIXES ${_glog_libpath_suffixes}
    )
    mark_as_advanced(${_name})
endfunction()

if(NOT DEFINED GLOG_MSVC_SEARCH)
    set(GLOG_MSVC_SEARCH MD)
endif()

set(_glog_libpath_suffixes lib)
if(MSVC)
    if(GLOG_MSVC_SEARCH STREQUAL "MD")
        list(APPEND _glog_libpath_suffixes
            msvc/glog-md/Debug
            msvc/glog-md/Release)
    elseif(GLOG_MSVC_SEARCH STREQUAL "MT")
        list(APPEND _glog_libpath_suffixes
            msvc/glog/Debug
            msvc/glog/Release)
    endif()
endif()


find_path(GLOG_INCLUDE_DIR glog/logging.h
    HINTS
        $ENV{GLOG_ROOT}/include
        ${GLOG_ROOT}/include
)
mark_as_advanced(GLOG_INCLUDE_DIR)

if(MSVC AND GLOG_MSVC_SEARCH STREQUAL "MD")
    # The provided /MD project files for Google Log add -md suffixes to the
    # library names.
    _glog_find_library(GLOG_LIBRARY            glog-md  glog)
else()
    _glog_find_library(GLOG_LIBRARY            glog)
endif()

INCLUDE(FindPackageHandleStandardArgs)
FIND_PACKAGE_HANDLE_STANDARD_ARGS(GLog DEFAULT_MSG GLOG_LIBRARY)

if(GLOG_FOUND)
    set(GLOG_INCLUDE_DIRS ${GLOG_INCLUDE_DIR})
    _glog_append_debugs(GLOG_LIBRARIES      GLOG_LIBRARY)
endif()
