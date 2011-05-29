add_executable(PythonInitTest PythonInitTest.cc)

include_directories(/usr/include/python2.6)

target_link_libraries(PythonInitTest
	boost_python
	python2.6
	glog
	gtest
	gmock
)

add_test(PythonInitTest PythonInitTest)