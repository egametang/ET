#ifndef NET_RPC_COMMUNICATOR_H
#define NET_RPC_COMMUNICATOR_H

#include <boost/asio.hpp>

namespace Hainan {

class RpcRequest;

class RpcCommunicator
{
private:
	boost::asio::io_service io_service;
	boost::asio::ip::tcp::socket socket;
public:
	RpcCommunicator(std::string& host, int port);
	~RpcCommunicator();
	void AsyncWrite(boost::asio::buffer& buffer,
			boost::function<void (const boost::asio::error_code&)> handler);
};

}

#endif // NET_RPC_COMMUNICATOR_H
