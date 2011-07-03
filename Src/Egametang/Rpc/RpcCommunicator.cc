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

void RpcCommunicator::RecvMeta()
{
	RpcMetaPtr meta(new RpcMeta());
	boost::asio::async_read(socket_,
			boost::asio::buffer(reinterpret_cast<char*>(meta.get()), sizeof(*meta)),
			boost::bind(&RpcCommunicator::RecvMessage, this, meta,
					boost::asio::placeholders::error));
}

void RpcCommunicator::RecvMessage(RpcMetaPtr meta, const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "receive message size failed: " << err.message();
		return;
	}
	StringPtr message(new std::string(meta->size, '\0'));
	boost::asio::async_read(socket_,
			boost::asio::buffer(reinterpret_cast<char*>(&message->at(0)), meta->size),
			boost::bind(&RpcCommunicator::RecvDone, this,
					meta, message,
					boost::asio::placeholders::error));
}

void RpcCommunicator::RecvDone(RpcMetaPtr meta, StringPtr message,
		const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "receive message failed: " << err.message();
		return;
	}
	OnRecvMessage(meta, message);
}

void RpcCommunicator::SendMeta(RpcMeta meta, std::string message)
{
	CHECK_EQ(meta.size, message.size()) << "meta and message size not match!";
	boost::asio::async_write(socket_,
			boost::asio::buffer(reinterpret_cast<char*>(&meta), sizeof(meta)),
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
			boost::bind(&RpcCommunicator::SendDone, this,
					boost::asio::placeholders::error));
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

void RpcCommunicator::OnRecvMessage(RpcMetaPtr meta, StringPtr message)
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
