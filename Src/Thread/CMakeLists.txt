set(ThreadSrc ThreadPool.cc)

add_library(Thread ${ThreadSrc})

add_executable(ThreadPoolTest ThreadPoolTest.cc)

target_link_libraries(ThreadPoolTest
    Thread
    gtest
    glog
    boost_thread
)