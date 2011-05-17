add_executable(PythonInitTest PythonInitTest.cc)

include_directories(/usr/include/python2.5)

target_link_libraries(PythonInitTest
    boost_python
    python2.5
    glog
    gtest
    gmock
)