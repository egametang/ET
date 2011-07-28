#ifndef RPC_RPCSERVERMOCK_H
#define RPC_RPCSERVERMOCK_H

#include <boost/asio.hpp>
#include <boost/function.hpp>
#include <boost/unordered_set.hpp>
#include <boost/unordered_map.hpp>
#include <google/protobuf/service.h>
#include <gmock/gmock.h>
#include "Rpc/RpcServer.h"

namespace Egametang {

class RpcServerMock: public RpcServer
{
public:
	RpcServerMock(boost::asio::io_service& io_service, int port):
		RpcServer(io_service, port)
	{
	}

	MOCK_METHOD0(IOService, boost::asio::io_service&());
	MOCK_METHOD4(RunService, void(RpcSessionPtr, RpcMetaPtr, StringPtr, MessageHandler));
	MOCK_METHOD1(RegisterService, void(RpcServicePtr));
	MOCK_METHOD1(RemoveSession, void(RpcSessionPtr&));
	MOCK_METHOD0(RemoveSession, void());
};

} // namespace Egametang

#endif // RPC_RPCSERVERMOCK_H
