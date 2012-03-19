FILE(GLOB Header "*.h")

SET(LogSrc 
	${Header}
	Log.cc
)

ADD_LIBRARY(Log ${LogSrc})
SET_PROPERTY(TARGET Log PROPERTY FOLDER "Platform")

ADD_EXECUTABLE(LogTest LogTest.cc)

SET(Tests
	LogTest
)

FOREACH(Test ${Tests})
	TARGET_LINK_LIBRARIES(${Test}
		Log
		${ThirdPartyLibs}
	)
	ADD_TEST(${Test} ${Test})
	SET_PROPERTY(TARGET ${Test} PROPERTY FOLDER "Tests/Platform/Log")
ENDFOREACH()