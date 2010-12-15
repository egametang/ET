#ifndef NET_RPC_SERVER_H
#define NET_RPC_SERVER_H

namespace Hainan {

class RpcServer
{
private:
	typedef boost::unordered_map<std::string, RpcHandlerPtr> RpcServiceMap;
	typedef boost::unordered_set<RpcSessionPtr> RpcSessionSet;

	RpcServiceMap services;
	boost::asio::io_service io_service;
	boost::asio::ip::tcp::acceptor acceptor;
	ThreadPool thread_pool;
	RpcSessionSet sessions;

public:
	RpcServer();
	~RpcServer();
	void Start();
	void Stop();
};

} // namespace Hainan

#endif // NET_RPC_SERVER_H
