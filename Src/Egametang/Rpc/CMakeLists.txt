PROTOBUF_GENERATE_CPP(proto_srcs proto_hdrs 
	Echo.proto
	)

SET(RpcSrc 
	RpcCommunicator.cc
	RpcController.cc
	RpcHandler.cc
	RpcChannel.cc
	${proto_srcs}
	)


ADD_LIBRARY(Rpc ${RpcSrc})

ADD_EXECUTABLE(RpcCommunicatorTest RpcCommunicatorTest.cc)
ADD_EXECUTABLE(RpcChannelTest RpcChannelTest.cc)

SET(Excutes 
	RpcCommunicatorTest
	RpcChannelTest
)

FOREACH(Excute ${Excutes})
	TARGET_LINK_LIBRARIES(${Excute}
		Rpc
		Thread
		Glog
		Gflags
		Gtest
		Gmock
		${Boost_LIBRARIES}
		${PYTHON_LIBRARY}
		${PROTOBUF_LIBRARY}
	)
	ADD_TEST(${Excute} ${Excute})
ENDFOREACH()