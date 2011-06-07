set(PythonSrc 
	PythonInit.cc 
	PythonEntry.cc
)
add_library(PythonEntry ${PythonSrc})

include_directories(/usr/include/python2.5)

add_executable(PythonInitTest PythonInitTest.cc)
add_executable(PythonEntryTest PythonEntryTest.cc)

set(Excutes 
	PythonInitTest 
	PythonEntryTest
)

foreach(Excute ${Excutes})
	target_link_libraries(${Excute}
		PythonEntry
		boost_python
		python2.5
		glog
		gtest
		gmock
	)
	add_test(${Excute} ${Excute})
endforeach()