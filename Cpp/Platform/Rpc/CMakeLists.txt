PROTOBUF_GENERATE_CPP(proto_srcs proto_hdrs 
	Echo.proto
)

SET(RpcSrc 
	RpcCommunicator.cc
	RpcController.cc
	RequestHandler.cc
	ResponseHandler.cc
	RpcChannel.cc
	RpcServer.cc
	RpcSession.cc
	${proto_srcs}
)


ADD_LIBRARY(Rpc ${RpcSrc})

ADD_EXECUTABLE(RpcCommunicatorTest RpcCommunicatorTest.cc)
ADD_EXECUTABLE(RpcChannelTest RpcChannelTest.cc)
ADD_EXECUTABLE(RpcSessionTest RpcSessionTest.cc)
ADD_EXECUTABLE(RpcServerTest RpcServerTest.cc)

SET(Excutes 
	RpcCommunicatorTest
	RpcChannelTest
	RpcSessionTest
	RpcServerTest
)

FOREACH(Excute ${Excutes})
	TARGET_LINK_LIBRARIES(${Excute}
		Rpc
		Thread
		${ThirdPartyLibs}
	)
	ADD_TEST(${Excute} ${Excute})
ENDFOREACH()