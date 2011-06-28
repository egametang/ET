SET(Gflags 
	src/gflags_completions.cc
	src/gflags_nc.cc
	src/gflags_reporting.cc
	src/gflags.cc
)

INCLUDE_DIRECTORIES(./src)

ADD_LIBRARY(Gflags ${Gflags})