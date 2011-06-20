set(ThreadSrc 
	ThreadPool.cc
	CountBarrier.cc)

add_library(Thread ${ThreadSrc})

add_executable(ThreadPoolTest ThreadPoolTest.cc)
add_executable(CountBarrierTest CountBarrierTest.cc)

set(Excutes 
	ThreadPoolTest 
	CountBarrierTest
)

foreach(Excute ${Excutes})
	target_link_libraries(${Excute}
		Thread
		boost_thread
		glog
		gflags
		gtest
		gmock
	)
	add_test(${Excute} ${Excute})
endforeach()