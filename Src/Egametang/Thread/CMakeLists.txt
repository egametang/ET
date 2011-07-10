SET(ThreadSrc 
	ThreadPool.cc
	CountBarrier.cc)

ADD_LIBRARY(Thread ${ThreadSrc})

ADD_EXECUTABLE(ThreadPoolTest ThreadPoolTest.cc)
ADD_EXECUTABLE(CountBarrierTest CountBarrierTest.cc)

SET(Excutes 
	ThreadPoolTest 
	CountBarrierTest
)

FOREACH(Excute ${Excutes})
	TARGET_LINK_LIBRARIES(${Excute}
		Thread
		${LinkLibs}
	)
	ADD_TEST(${Excute} ${Excute})
ENDFOREACH()