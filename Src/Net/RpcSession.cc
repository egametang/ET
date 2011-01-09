#include "Net/RpcSession.h"

namespace Hainan {

RpcSession::RpcSession(RpcSessionSet& rpc_sessions): sessions(rpc_sessions)
{
}

boost::asio::ip::tcp::socket& RpcSession::Socket()
{
	return socket;
}

void RpcSession::SendMessageHandler(int32 id, RpcHandlerPtr handler,
		const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "SendMessage error:";
		return;
	}
	handlers[id] = handler;
}

void RpcSession::SendMessage(const RpcResponsePtr response, const boost::system::error_code& err)
{
	if (err)
	{
		return;
	}
	std::string ss = response->SerializeAsString();
	boost::asio::async_write(socket, boost::asio::buffer(ss),
			boost::bind(&RpcSession::SendMessageHandler, this,
					response->id(), boost::asio::placeholders::error));
}

void RpcSession::SendMessageSize(RpcResponsePtr response)
{
	int size = response->ByteSize();
	std::string ss = boost::lexical_cast(size);
	boost::asio::async_write(socket, boost::asio::buffer(ss),
			boost::bind(&RpcSession::SendMessage, this,
					response, boost::asio::placeholders::error));
}
///////////////////////////

void RpcSession::RecvMessegeSize()
{
	IntPtr size(new int);
	boost::asio::async_read(socket,
			boost::asio::buffer(
					reinterpret_cast<char*>(size.get()), sizeof(int)),
			boost::bind(&RpcChannel::RecvMessage, this, size,
					boost::asio::placeholders::error));
}

void RpcSession::RecvMessage(IntPtr size, const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "receive request size failed";
		return;
	}
	StringPtr ss(new std::string);
	boost::asio::async_read(socket,
			boost::asio::buffer(*ss, *size),
			boost::bind(&RpcSession::RecvMessageHandler, this, ss,
					boost::asio::placeholders::error));
}

void RpcSession::RecvMessageHandler(StringPtr ss, const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "receive request message failed";
		return;
	}

	RpcRequestPtr request(new RpcRequest);
	request->ParseFromString(*ss);

	RpcResponsePtr response(new RpcResponse);
	response->set_id(request->id());

	rpc_server.RunService(shared_from_this(), request,
			boost::bind(&RpcSession::SendMessegeSize, shared_from_this(), response));

	// read size
	RecvMessegeSize();
}

void RpcSession::Start()
{
	RecvMessegeSize();
}

void RpcSession::Stop()
{
	socket.close();
	sessions.erase(shared_from_this());
}

}
