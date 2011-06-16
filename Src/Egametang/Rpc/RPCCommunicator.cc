#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include "Rpc/RpcCommunicator.h"

namespace Egametang {

RPCCommunicator::RPCCommunicator()
{
}

boost::asio::ip::tcp::socket& RPCCommunicator::Socket()
{
	return socket_;
}

void RPCCommunicator::RecvSize()
{
	IntPtr size(new int);
	boost::asio::async_read(socket_,
			boost::asio::buffer(reinterpret_cast<char*>(size.get()), sizeof(int)),
			boost::bind(&RPCCommunicator::RecvMessage, this, size,
					boost::asio::placeholders::error));
}

void RPCCommunicator::RecvMessage(IntPtr size, const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "receive message size failed: " << err.message();
		return;
	}
	StringPtr ss;
	boost::asio::async_read(socket_,
			boost::asio::buffer(*ss, *size),
			boost::bind(&RPCCommunicator::RecvDone, this, ss,
					boost::asio::placeholders::error));
}

void RPCCommunicator::RecvDone(StringPtr ss, const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "receive message failed: " << err.message();
		return;
	}
	OnRecvMessage(ss);
}

void RPCCommunicator::SendSize(int size, std::string message)
{
	std::string ssize = boost::lexical_cast(size);
	boost::asio::async_write(socket_, boost::asio::buffer(ssize),
			boost::bind(&RPCCommunicator::SendMessage, this, message,
					handler, boost::asio::placeholders::error));
}

void RPCCommunicator::SendMessage(std::string message, const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "send message size failed: " << err.message();
		return;
	}
	boost::asio::async_write(socket_, boost::asio::buffer(message),
			boost::bind(&RPCCommunicator::SendDone, this, boost::asio::placeholders::error));
}

void RPCCommunicator::SendDone(const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "send message failed: " << err.message();
		return;
	}
	OnSendMessage();
}

} // namespace Egametang
