SET(PythonSrc 
	PythonInit.cc 
	PythonEntry.cc
)
ADD_LIBRARY(PythonEntry ${PythonSrc})

ADD_EXECUTABLE(PythonInitTest PythonInitTest.cc)
ADD_EXECUTABLE(PythonEntryTest PythonEntryTest.cc)

SET(Excutes 
	PythonInitTest 
	PythonEntryTest
)

FOREACH(Excute ${Excutes})
	TARGET_LINK_LIBRARIES(${Excute}
		PythonEntry
		Glog
		Gflags
		Gtest
		Gmock
		${Boost_LIBRARIES}
		${PYTHON_LIBRARY}
	)
	ADD_TEST(${Excute} ${Excute})
ENDFOREACH()