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
		boost_python
		${PYTHON_LIBRARIES}
		Glog
		Gflags
		Gtest
		Gmock
	)
	ADD_TEST(${Excute} ${Excute})
ENDFOREACH()