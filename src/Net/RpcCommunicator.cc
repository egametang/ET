#include "Net/RpcCommunicator.h"

namespace Hainan {

RpcCommunicator::RpcCommunicator(std::string& host, int port):
		io_service(), socket(io_service)
{
	boost::asio::ip::address address;
	address.from_string(host);
	boost::asio::ip::tcp::endpoint endpoint(address, port);
	socket.async_connect(endpoint,
			boost::bind(&RpcCommunicator::AsynConnectHandler, this,
					boost::asio::placeholders::error));
}

RpcCommunicator::~RpcCommunicator()
{
}

void RpcCommunicator::AsyncWrite(boost::asio::buffer buffer,
		boost::function<void (const boost::asio::error_code&)> handler)
{
	boost::asio::async_write(socket, buffer, handler);
}

} // namespace Hainan
