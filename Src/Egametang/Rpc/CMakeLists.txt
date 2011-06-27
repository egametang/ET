protobuf_generate_cpp(proto_srcs proto_hdrs RpcProtobufData.proto)

set(RpcSrc 
	RPCCommunicator.cc
	${proto_srcs}
	)

add_library(Rpc ${RpcSrc})

add_executable(RpcCommunicatorTest RPCCommunicatorTest.cc)

set(Excutes 
	RpcCommunicatorTest
)

foreach(Excute ${Excutes})
	target_link_libraries(${Excute}
		Rpc
		Thread
		${protobuf_library}
		boost_thread
		boost_system
		Glog
		Gflags
		Gtest
		Gmock
	)
	add_test(${Excute} ${Excute})
endforeach()