PROTOBUF_GENERATE_CPP(ProtoSrcs ProtoHdrs 
	Echo.proto
)

FILE(GLOB Header "*.h")

SET(RpcSrc 
	${Header}
	${ProtoHdrs}
	${ProtoSrcs}
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

SET(Tests 
	RpcCommunicatorTest
#	RpcClientTest
#	RpcSessionTest
#	RpcServerTest
)

FOREACH(Test ${Tests})
	TARGET_LINK_LIBRARIES(${Test}
		Log
		Rpc
		Thread
		${ThirdPartyLibs}
	)
	ADD_TEST(${Test} ${Test})
	SET_PROPERTY(TARGET ${Test} PROPERTY FOLDER "Tests/Platform/Rpc")
ENDFOREACH()