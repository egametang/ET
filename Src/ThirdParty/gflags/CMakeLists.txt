set(Gflags 
	src/gflags_completions.cc
	src/gflags_nc.cc
	src/gflags_reporting.cc
	src/gflags.cc
)

include_directories(./src)

add_library(Gflags ${Gflags})