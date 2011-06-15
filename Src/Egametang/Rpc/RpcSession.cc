#include "Rpc/RpcSession.h"

namespace Egametang {

RpcSession::RpcSession(RpcServer& rpc_server): rpc_server_(rpc_server)
{
}

boost::asio::ip::tcp::socket& RpcSession::Socket()
{
	return socket_;
}

void RpcSession::OnSendMessage(int32 id, RpcHandlerPtr handler,
		const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "SendMessage error:";
		return;
	}
}

void RpcSession::OnRecvMessage(StringPtr ss, const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "receive request message failed";
		return;
	}

	RpcRequestPtr request(new RpcRequest);
	request->ParseFromString(*ss);

	RpcResponsePtr response(new RpcResponse);
	response->set_id(request->id_());

	rpc_server_.RunService(shared_from_this(), request,
			boost::bind(&RPCCommunicator::SendMessegeSize, shared_from_this(), response));

	// read size
	RecvMessegeSize();
}

void RpcSession::Start()
{
	RecvMessegeSize();
}

void RpcSession::Stop()
{
	socket_.close();
	rpc_server_.RemoveSession(shared_from_this());
}

}
