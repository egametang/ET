#include "Rpc/RpcCommunicator.h"

namespace Egametang {

RPCCommunicator::RPCCommunicator()
{
}

void RPCCommunicator::RecvMessegeSize()
{
	IntPtr size(new int);
	boost::asio::async_read(socket_,
			boost::asio::buffer(
					reinterpret_cast<char*>(size.get()), sizeof(int)),
			boost::bind(&RPCCommunicator::RecvMessage, this, size,
					boost::asio::placeholders::error));
}

void RPCCommunicator::RecvMessage(IntPtr size, const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "receive response size failed";
		return;
	}
	StringPtr ss;
	boost::asio::async_read(socket_,
			boost::asio::buffer(*ss, *size),
			boost::bind(&RPCCommunicator::OnRecvMessage, this, ss,
					boost::asio::placeholders::error));
}


void RPCCommunicator::SendMessage(const RpcRequestPtr request,
		RpcHandlerPtr handler, const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "SendRequestSize error:";
		return;
	}
	std::string ss = request->SerializeAsString();
	boost::asio::async_write(socket_, boost::asio::buffer(ss),
			boost::bind(&RPCCommunicator::OnSendMessage, this, request->id(),
					handler, boost::asio::placeholders::error));
}

void RPCCommunicator::SendMessageSize(
		const RpcRequestPtr request, RpcHandlerPtr handler)
{
	int size = request->ByteSize();
	std::string ss = boost::lexical_cast(size);
	boost::asio::async_write(socket_, boost::asio::buffer(ss),
			boost::bind(&RPCCommunicator::SendMessage, this, request,
					handler, boost::asio::placeholders::error));
}

} // namespace Egametang
