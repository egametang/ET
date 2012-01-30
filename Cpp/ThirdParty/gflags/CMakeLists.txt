IF(MSVC)
	SET(GflagsSrc
		src/gflags.cc
		src/gflags_reporting.cc
		src/gflags_completions.cc
		src/windows/port.cc
		src/mutex.h
		src/windows/gflags.h
		src/windows/gflags_declare.h
		src/windows/gflags_completions.h
		src/windows/config.h
		src/windows/port.h
	)
ELSEIF(UNIX)
	SET(GflagsSrc
		src/gflags.cc
		src/gflags_reporting.cc
		src/gflags_completions.cc
		src/mutex.h
		src/linux/gflags.h
		src/linux/gflags_declare.h
		src/linux/gflags_completions.h
		src/linux/config.h
	)
ENDIF()

INCLUDE_DIRECTORIES(src)
ADD_LIBRARY(gflags ${GflagsSrc})
SET_PROPERTY(TARGET gflags PROPERTY FOLDER "ThirdParty")