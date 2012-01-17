FILE(GLOB Header "*.h")

SET(MonoSrc 
	${Header}
	MonoInit.cc
)

ADD_LIBRARY(Mono ${MonoSrc})
SET_PROPERTY(TARGET Rpc PROPERTY FOLDER "Platform")

IF(UNIX)
	SET(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} ${MONO_CXX_FLAGS}")
	SET(CMAKE_LINK_LIBRARY_FLAG "${CMAKE_LINK_LIBRARY_FLAG} ${MONO_LINK_FLAGS}")
ENDIF()

FILE(GLOB MonoCsharpSrc "*.cs")
MESSAGE(STATUS ${MonoCsharpSrc})
ADD_CUSTOM_COMMAND(
	OUTPUT MonoInit.exe
	COMMAND ${GMCS_EXECUTABLE} 
	ARGS ${MonoCsharpSrc} -out:${CMAKE_CURRENT_BINARY_DIR}/MonoInit.exe
	DEPENDS ${MonoCsharpSrc}
)

ADD_CUSTOM_TARGET(MonoAssembly
	DEPENDS MonoInit.exe
)

ADD_EXECUTABLE(MonoInitTest MonoInitTest.cc)

ADD_DEPENDENCIES(MonoInitTest MonoAssembly)

SET(Tests 
	MonoInitTest
)

FOREACH(Test ${Tests})
	TARGET_LINK_LIBRARIES(${Test}
		Mono
		${ThirdPartyLibs}
	)
	ADD_TEST(${Test} ${Test})
	SET_PROPERTY(TARGET ${Test} PROPERTY FOLDER "Tests/Platform/Mono")
ENDFOREACH()