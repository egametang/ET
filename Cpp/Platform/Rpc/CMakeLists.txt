PROTOBUF_GENERATE_CPP(ProtoSrcs ProtoHdrs 
	Echo.proto
)

FILE(GLOB Header "*.h")

SET(RpcSrc 
	${ProtoHdrs}
	${ProtoSrcs}
	${Header}
	MethodInfo.cc
	RpcCommunicator.cc
	RpcController.cc
	RequestHandler.cc
	ResponseHandler.cc
	RpcClient.cc
	RpcServer.cc
	RpcSession.cc
)


ADD_LIBRARY(Rpc ${RpcSrc})
SET_PROPERTY(TARGET Rpc PROPERTY FOLDER "Platform")

ADD_EXECUTABLE(RpcCommunicatorTest RpcCommunicatorTest.cc)
#ADD_EXECUTABLE(RpcClientTest RpcClientTest.cc)
#ADD_EXECUTABLE(RpcSessionTest RpcSessionTest.cc)
#ADD_EXECUTABLE(RpcServerTest RpcServerTest.cc)

TARGET_LINK_LIBRARIES(RpcCommunicatorTest
	Log
	Rpc
	Thread
	${ThirdPartyLibs}
)
ADD_TEST(RpcCommunicatorTest RpcCommunicatorTest)
SET_PROPERTY(TARGET RpcCommunicatorTest PROPERTY FOLDER "Tests/Platform/Rpc")