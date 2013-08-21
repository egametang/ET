FILE(GLOB Header "*.h")

SET(LogSrc 
	${Header}
	Log.cc
)

ADD_LIBRARY(Log ${LogSrc})
SET_PROPERTY(TARGET Log PROPERTY FOLDER "Platform")

ADD_EXECUTABLE(LogTest LogTest.cc)

TARGET_LINK_LIBRARIES(LogTest
	Log
	${ThirdPartyLibs}
)
ADD_TEST(LogTest LogTest)
SET_PROPERTY(TARGET LogTest PROPERTY FOLDER "Tests/Platform/Log")