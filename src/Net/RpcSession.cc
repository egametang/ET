#include "Net/RpcSession.h"

namespace Hainan {

RpcSession::RpcSession(RpcSessionSet& rpc_sessions): sessions(rpc_sessions)
{
}

boost::asio::ip::tcp::socket& RpcSession::Socket()
{
	return socket;
}

void RpcSession::RecvRequestSize()
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
	StringPtr ss;
	boost::asio::async_read(socket,
			boost::asio::buffer(*ss, *size),
			boost::bind(&RpcSession::RecvMessageHandler, this, ss,
					boost::asio::placeholders::error));
}

void RpcSession::RecvMessageHandler(StringPtr ss,
		const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "receive request message failed";
		return;
	}

	RpcRequest request;
	request->ParseFromString(*ss);


	// read size
	RecvRequestSize();
}

void RpcSession::Start()
{
	RecvRequestSize();
}

void RpcSession::Stop()
{
	socket.close();
	sessions.erase(shared_from_this());
}

}
