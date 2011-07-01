#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include <boost/lexical_cast.hpp>
#include <glog/logging.h>
#include "Rpc/RpcCommunicator.h"

namespace Egametang {

RpcCommunicator::RpcCommunicator(boost::asio::io_service& io_service):
		io_service_(io_service), socket_(io_service)
{
}

boost::asio::ip::tcp::socket& RpcCommunicator::Socket()
{
	return socket_;
}

void RpcCommunicator::RecvSize()
{
	IntPtr size(new int(0));
	boost::asio::async_read(socket_,
			boost::asio::buffer(reinterpret_cast<char*>(size.get()), sizeof(int)),
			boost::bind(&RpcCommunicator::RecvMessage, this, size,
					boost::asio::placeholders::error));
}

void RpcCommunicator::RecvMessage(IntPtr size, const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "receive message size failed: " << err.message();
		return;
	}
	StringPtr ss(new std::string(*size, '\0'));
	boost::asio::async_read(socket_,
			boost::asio::buffer(reinterpret_cast<char*>(&ss->at(0)), *size),
			boost::bind(&RpcCommunicator::RecvDone, this, ss,
					boost::asio::placeholders::error));
}

void RpcCommunicator::RecvDone(StringPtr ss, const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "receive message failed: " << err.message();
		return;
	}
	OnRecvMessage(ss);
}

void RpcCommunicator::SendSize(int size, std::string message)
{
	boost::asio::async_write(socket_,
			boost::asio::buffer(reinterpret_cast<char*>(&size), sizeof(int)),
			boost::bind(&RpcCommunicator::SendMessage, this, message,
					boost::asio::placeholders::error));
}

void RpcCommunicator::SendMessage(std::string message, const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "send message size failed: " << err.message();
		return;
	}
	boost::asio::async_write(socket_, boost::asio::buffer(message),
			boost::bind(&RpcCommunicator::SendDone, this, boost::asio::placeholders::error));
}

void RpcCommunicator::SendDone(const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "send message failed: " << err.message();
		return;
	}
	OnSendMessage();
}

void RpcCommunicator::OnRecvMessage(StringPtr ss)
{
}

void RpcCommunicator::OnSendMessage()
{
}

void RpcCommunicator::Stop()
{
	socket_.close();
}

} // namespace Egametang
