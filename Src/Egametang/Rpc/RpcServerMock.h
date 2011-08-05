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
	RpcServerMock(int port): RpcServer(port)
	{
	}

	MOCK_METHOD4(RunService, void(RpcSessionPtr, RpcMetaPtr, StringPtr, MessageHandler));
	MOCK_METHOD1(Register, void(RpcServicePtr));
	MOCK_METHOD1(Remove, void(RpcSessionPtr&));
};

} // namespace Egametang

#endif // RPC_RPCSERVERMOCK_H
