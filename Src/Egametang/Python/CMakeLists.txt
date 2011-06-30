SET(PythonSrc 
	PythonInit.cc 
	PythonInterpreter.cc
)
ADD_LIBRARY(PythonInterpreter ${PythonSrc})

ADD_EXECUTABLE(PythonInitTest PythonInitTest.cc)
ADD_EXECUTABLE(PythonInterpreterTest PythonInterpreterTest.cc)

SET(Excutes 
	PythonInitTest 
	PythonInterpreterTest
)

FOREACH(Excute ${Excutes})
	TARGET_LINK_LIBRARIES(${Excute}
		PythonInterpreter
		Glog
		Gflags
		Gtest
		Gmock
		${Boost_LIBRARIES}
		${PYTHON_LIBRARY}
	)
	ADD_TEST(${Excute} ${Excute})
ENDFOREACH()