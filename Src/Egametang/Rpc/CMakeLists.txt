set(RpcSrc 
	RPCCommunicator.cc
	)

add_library(Rpc ${RpcSrc})

add_executable(RPCCommunicatorTest RPCCommunicatorTest.cc)

set(Excutes 
	RPCCommunicatorTest
)

foreach(Excute ${Excutes})
	target_link_libraries(${Excute}
		Rpc
		boost_system
		protobuf
		glog
		gtest
		gmock
	)
	add_test(${Excute} ${Excute})
endforeach()