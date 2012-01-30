IF(MSVC)
	SET(GlogSrc
		src/logging.cc
		src/windows/port.cc
		src/raw_logging.cc
		src/utilities.cc
		src/vlog_is_on.cc
		src/utilities.h
		src/base/commandlineflags.h
		src/base/googleinit.h
		src/base/mutex.h
		src/windows/port.h
		src/windows/config.h
		src/windows/log_severity.h
		src/windows/logging.h
		src/windows/raw_logging.h
		src/windows/stl_logging.h
	)
	SET(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} /wd4273")
ELSEIF(UNIX)
	SET(GlogSrc
		src/windows/vlog_is_on.h
		src/logging.cc 
		src/raw_logging.cc 
		src/vlog_is_on.cc 
		src/utilities.cc 
		src/utilities.h 
		src/demangle.cc 
		src/demangle.h 
		src/stacktrace.h 
		src/stacktrace_generic-inl.h 
		src/stacktrace_libunwind-inl.h 
		src/stacktrace_powerpc-inl.h 
		src/stacktrace_x86-inl.h 
		src/stacktrace_x86_64-inl.h 
		src/symbolize.cc 
		src/symbolize.h 
		src/signalhandler.cc 
		src/base/mutex.h 
		src/base/googleinit.h 
		src/base/commandlineflags.h
	)
ENDIF()

INCLUDE_DIRECTORIES(src)
INCLUDE_DIRECTORIES(src/windows)
ADD_LIBRARY(glog ${GlogSrc})
SET_PROPERTY(TARGET glog PROPERTY FOLDER "ThirdParty")