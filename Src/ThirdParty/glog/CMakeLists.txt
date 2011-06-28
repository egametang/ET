SET(Glog
	src/demangle.cc
	src/logging.cc
	src/raw_logging.cc
	src/signalhandler.cc
	src/symbolize.cc
	src/utilities.cc
	src/vlog_is_on.cc
)

INCLUDE_DIRECTORIES(./src)

ADD_LIBRARY(Glog ${Glog})