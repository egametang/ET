FILE(GLOB Header "*.h")

SET(MonoSrc 
	${Header}
	MonoInit.cc
)

ADD_LIBRARY(Mono ${MonoSrc})
SET_PROPERTY(TARGET Mono PROPERTY FOLDER "Platform")

IF(MSVC)
	SET(MonoLibRelativeDir ${CMAKE_BUILD_TYPE})
ELSEIF(UNIX)
	SET(MonoLibRelativeDir "")
ENDIF()

FILE(GLOB MonoCsharpSrc "*.cs")
ADD_CUSTOM_COMMAND(
	OUTPUT MonoInit.exe
	COMMAND ${MONO_MCS_EXECUTABLE} 
	ARGS ${MonoCsharpSrc} -out:${CMAKE_CURRENT_BINARY_DIR}/${MonoLibRelativeDir}/MonoInit.exe
	DEPENDS ${MonoCsharpSrc}
)

ADD_CUSTOM_TARGET(MonoInitTestAssembly ALL
	DEPENDS MonoInit.exe
)
SET_PROPERTY(TARGET MonoInitTestAssembly PROPERTY FOLDER "Tests/Platform/Mono")

INCLUDE_DIRECTORIES(${MONO_INCLUDE_DIRS})

ADD_EXECUTABLE(MonoInitTest MonoInitTest.cc)

ADD_DEPENDENCIES(MonoInitTest MonoInitTestAssembly)

SET(Tests
	MonoInitTest
)

FOREACH(Test ${Tests})
	TARGET_LINK_LIBRARIES(${Test}
		${Mono}
		${MONO_LIBRARIES}
		${ThirdPartyLibs}
	)
	ADD_TEST(${Test} ${Test})
	SET_PROPERTY(TARGET ${Test} PROPERTY FOLDER "Tests/Platform/Mono")
ENDFOREACH()