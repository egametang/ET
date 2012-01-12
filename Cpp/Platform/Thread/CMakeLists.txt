FILE(GLOB Header "*.h")

SET(ThreadSrc 
	${Header}
	ThreadPool.cc
	CountBarrier.cc
)

ADD_LIBRARY(Thread ${ThreadSrc})

ADD_EXECUTABLE(ThreadPoolTest ThreadPoolTest.cc)
ADD_EXECUTABLE(CountBarrierTest CountBarrierTest.cc)

SET(Tests 
	ThreadPoolTest 
	CountBarrierTest
)

FOREACH(Test ${Tests})
	TARGET_LINK_LIBRARIES(${Test}
		Thread
		${ThirdPartyLibs}
	)
	ADD_TEST(${Test} ${Test})
	SET_PROPERTY(TARGET ${Test} PROPERTY FOLDER "Tests")
ENDFOREACH()