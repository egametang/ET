PROTOBUF_GENERATE_CPP(proto_srcs proto_hdrs RpcProtobufData.proto)

SET(RpcSrc 
	RPCCommunicator.cc
	${proto_srcs}
	)

ADD_LIBRARY(Rpc ${RpcSrc})

ADD_EXECUTABLE(RpcCommunicatorTest RPCCommunicatorTest.cc)

SET(Excutes 
	RpcCommunicatorTest
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