SET(GmockSrc
	src/gmock-all.cc
)
INCLUDE_DIRECTORIES(.)
ADD_LIBRARY(gmock ${GmockSrc})
SET_PROPERTY(TARGET gmock PROPERTY FOLDER "ThirdParty")