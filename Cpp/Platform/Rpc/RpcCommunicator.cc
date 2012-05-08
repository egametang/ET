#include <functional>
#include <boost/asio.hpp>
#include <boost/lexical_cast.hpp>
#include "Rpc/RpcCommunicator.h"

namespace Egametang {

RpcCommunicator::RpcCommunicator(boost::asio::io_service& service):
		ioService(service), socket(service)
{
}

RpcCommunicator::~RpcCommunicator()
{
	socket.close();
}

boost::asio::ip::tcp::socket& RpcCommunicator::Socket()
{
	return socket;
}

void RpcCommunicator::Stop()
{
}


void RpcCommunicator::RecvMeta(RpcMetaPtr meta, StringPtr message)
{
	boost::asio::async_read(socket,
			boost::asio::buffer(reinterpret_cast<char*>(meta.get()), sizeof(*meta)),
			std::bind(&RpcCommunicator::RecvMessage, this,
					meta, message, std::placeholders::_1));
}

void RpcCommunicator::RecvMessage(RpcMetaPtr meta, StringPtr message,
		const boost::system::error_code& err)
{
	if (err)
	{
		Stop();
		return;
	}
	message->resize(meta->size, 0);
	boost::asio::async_read(socket,
			boost::asio::buffer(reinterpret_cast<char*>(&message->at(0)), meta->size),
			std::bind(&RpcCommunicator::RecvDone, this,
					meta, message, std::placeholders::_1));
}

void RpcCommunicator::RecvDone(RpcMetaPtr meta, StringPtr message,
		const boost::system::error_code& err)
{
	if (err)
	{
		Stop();
		return;
	}
	OnRecvMessage(meta, message);
}

void RpcCommunicator::OnRecvMessage(RpcMetaPtr meta, StringPtr message)
{
}


void RpcCommunicator::SendMeta(RpcMetaPtr meta, StringPtr message)
{
	boost::asio::async_write(socket,
			boost::asio::buffer(reinterpret_cast<char*>(meta.get()), sizeof(*meta)),
			std::bind(&RpcCommunicator::SendMessage, this,
					meta, message, std::placeholders::_1));
}

void RpcCommunicator::SendMessage(RpcMetaPtr meta, StringPtr message,
		const boost::system::error_code& err)
{
	if (err)
	{
		Stop();
		return;
	}
	boost::asio::async_write(socket, boost::asio::buffer(*message),
			std::bind(&RpcCommunicator::SendDone, this,
					meta, message, std::placeholders::_1));
}

void RpcCommunicator::SendDone(RpcMetaPtr meta, StringPtr message,
		const boost::system::error_code& err)
{
	if (err)
	{
		Stop();
		return;
	}
	OnSendMessage(meta, message);
}

void RpcCommunicator::OnSendMessage(RpcMetaPtr meta, StringPtr message)
{
}

} // namespace Egametang
