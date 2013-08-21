FILE(GLOB Header "*.h")

SET(ThreadSrc 
	${Header}
	CountBarrier.cc
)

ADD_LIBRARY(Thread ${ThreadSrc})
SET_PROPERTY(TARGET Thread PROPERTY FOLDER "Platform")

ADD_EXECUTABLE(CountBarrierTest CountBarrierTest.cc)

TARGET_LINK_LIBRARIES(CountBarrierTest
	Thread
	${ThirdPartyLibs}
)
ADD_TEST(CountBarrierTest CountBarrierTest)
SET_PROPERTY(TARGET CountBarrierTest PROPERTY FOLDER "Tests/Platform/Thread")