set(RpcSrc 
	RPCCommunicator.cc
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
		boost_thread
		boost_system
		protobuf
		glog
		gtest
		gmock
	)
	add_test(${Excute} ${Excute})
endforeach()