#ifndef NET_RPC_COMMUNICATOR_H
#define NET_RPC_COMMUNICATOR_H

#include <boost/asio.hpp>

namespace hainan {

class rpc_request;
class rpc_callback;

class rpc_communicator
{
private:
	boost::asio::ip::tcp::socket socket_;
public:
	rpc_communicator(boost::asio::ip::tcp::endpoint& endpoint);
	~rpc_communicator();
	void send_message(const rpc_request& req, rpc_callback& callback);
};

}

#endif // NET_RPC_COMMUNICATOR_H
