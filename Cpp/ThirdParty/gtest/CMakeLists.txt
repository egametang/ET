SET(GtestSrc
	src/gtest-all.cc
)
INCLUDE_DIRECTORIES(.)
ADD_LIBRARY(gtest ${GtestSrc})
SET_PROPERTY(TARGET gtest PROPERTY FOLDER "ThirdParty")