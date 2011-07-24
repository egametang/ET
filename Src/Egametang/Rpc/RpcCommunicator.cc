#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include <boost/lexical_cast.hpp>
#include <glog/logging.h>
#include "Rpc/RpcCommunicator.h"

namespace Egametang {

RpcCommunicator::RpcCommunicator(boost::asio::io_service& io_service):
		io_service(io_service), socket(io_service)
{
}

RpcCommunicator::~RpcCommunicator()
{
}

boost::asio::ip::tcp::socket& RpcCommunicator::Socket()
{
	return socket;
}

void RpcCommunicator::Stop()
{
	socket.close();
}


void RpcCommunicator::RecvMeta(RpcMetaPtr meta, StringPtr message)
{
	boost::asio::async_read(socket,
			boost::asio::buffer(reinterpret_cast<char*>(meta.get()), sizeof(*meta)),
			boost::bind(&RpcCommunicator::RecvMessage, this,
					meta, message, boost::asio::placeholders::error));
}

void RpcCommunicator::RecvMessage(RpcMetaPtr meta, StringPtr message,
		const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "receive message size failed: " << err.message();
		return;
	}
	message->resize(meta->size, '\0');
	boost::asio::async_read(socket,
			boost::asio::buffer(reinterpret_cast<char*>(&message->at(0)), meta->size),
			boost::bind(&RpcCommunicator::RecvDone, this,
					meta, message, boost::asio::placeholders::error));
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

void RpcCommunicator::OnRecvMessage(RpcMetaPtr meta, StringPtr message)
{
}


void RpcCommunicator::SendMeta(RpcMetaPtr meta, StringPtr message)
{
	CHECK_EQ(meta->size, message->size()) << "meta and message size not match!";
	boost::asio::async_write(socket,
			boost::asio::buffer(reinterpret_cast<char*>(meta.get()), sizeof(*meta)),
			boost::bind(&RpcCommunicator::SendMessage, this,
					meta, message, boost::asio::placeholders::error));
}

void RpcCommunicator::SendMessage(RpcMetaPtr meta, StringPtr message,
		const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "send message size failed: " << err.message();
		return;
	}
	boost::asio::async_write(socket, boost::asio::buffer(*message),
			boost::bind(&RpcCommunicator::SendDone, this,
					meta, message, boost::asio::placeholders::error));
}

void RpcCommunicator::SendDone(RpcMetaPtr meta, StringPtr message,
		const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "send message failed: " << err.message();
		return;
	}
	OnSendMessage(meta, message);
}

void RpcCommunicator::OnSendMessage(RpcMetaPtr meta, StringPtr message)
{
}

} // namespace Egametang
