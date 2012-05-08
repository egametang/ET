#ifndef RPC_RPCSERVERMOCK_H
#define RPC_RPCSERVERMOCK_H

#include <functional>
#include <unordered_set>
#include <unordered_map>
#include <boost/asio.hpp>
#include <google/protobuf/service.h>
#include <gmock/gmock.h>
#include "Rpc/RpcServer.h"

namespace Egametang {

class RpcServerMock: public RpcServer
{
public:
	RpcServerMock(boost::asio::io_service& service, int port):
		RpcServer(service, port)
	{
	}

	MOCK_METHOD4(RunService, void(RpcSessionPtr, const RpcMetaPtr,  const StringPtr, MessageHandler));
	MOCK_METHOD1(Register, void(ProtobufServicePtr));
	MOCK_METHOD1(Remove, void(RpcSessionPtr&));
};

} // namespace Egametang

#endif // RPC_RPCSERVERMOCK_H
