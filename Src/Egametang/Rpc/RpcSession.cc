#include "Rpc/RpcSession.h"

namespace Egametang {

RpcSession::RpcSession(RpcServer& rpc_server):
		rpc_server_(rpc_server), RpcCommunicator(rpc_server_.io_service_)
{
}

void RpcSession::OnRecvMessage(StringPtr ss)
{
	RpcRequestPtr request(new RpcRequest);
	request->ParseFromString(*ss);

	int size = request->ByteSize();
	std::string message = request->SerializeAsString();

	RpcResponsePtr response(new RpcResponse);
	response->set_id(request->id());

	rpc_server_.RunService(shared_from_this(), request,
			boost::bind(&RpcSession::SendResponse,
					shared_from_this(), response));
	// read size
	RecvSize();
}

void RpcSession::OnSendMessage()
{
}

void RpcSession::SendResponse(RpcResponsePtr response)
{
	int size = response->ByteSize();
	std::string message = response->SerializeAsString();
	SendSize(size, message);
}

void RpcSession::Start()
{
	RecvSize();
}

void RpcSession::Stop()
{
	socket_.close();
	rpc_server_.RemoveSession(shared_from_this());
}

} // namespace Egametang
