#ifndef NET_RPC_SERVER_H
#define NET_RPC_SERVER_H

namespace Hainan {

class RpcServer
{
private:
	typedef boost::unordered_set<RpcSessionPtr> RpcSessionSet;

	boost::asio::io_service io_service;
	boost::asio::ip::tcp::acceptor acceptor;
	RpcSessionSet sessions;

	void HandleAsyncAccept(RpcSessionSet session,
			const boost::system::error_code& err);
public:
	RpcServer();
	~RpcServer();
	void Start();
	void Stop();
};

} // namespace Hainan

#endif // NET_RPC_SERVER_H
