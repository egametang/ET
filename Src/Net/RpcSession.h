#ifndef NET_RPC_SESSION_H
#define NET_RPC_SESSION_H

#include <boost/asio.hpp>
#include <boost/array.hpp>
#include <boost/noncopyable.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>

namespace Hainan {

class RpcServer;

class RpcSession: private boost::noncopyable, public boost::enable_shared_from_this<RpcSession>
{
private:
	boost::asio::ip::tcp::socket socket;
	RpcServer& rpc_server;

public:
	RpcSession(RpcServer& server);
	~RpcSession();
	boost::asio::ip::tcp::socket& Socket();
	void Start();
	void Stop();
};

} // namespace Hainan

#endif // NET_RPC_SESSION_H
