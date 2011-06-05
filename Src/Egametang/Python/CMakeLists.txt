set(PythonSrc 
	PythonInit.cc 
	PythonEntry.cc
)
add_library(Python ${PythonSrc})

include_directories(/usr/include/python2.6)

add_executable(PythonInitTest PythonInitTest.cc)
add_executable(PythonEntryTest PythonEntryTest.cc)

set(Excutes 
	PythonInitTest 
	PythonEntryTest
)

foreach(Excute ${Excutes})
	target_link_libraries(${Excute}
		boost_python
		python2.6
		glog
		gtest
		gmock
	)
	add_test(${Excute} ${Excute})
endforeach()

#target_link_libraries(PythonInitTest
#	boost_python
#	python2.6
#	glog
#	gtest
#	gmock
#)
#
#target_link_libraries(PythonEntryTest
#	boost_python
#	python2.6
#	glog
#	gtest
#	gmock
#)