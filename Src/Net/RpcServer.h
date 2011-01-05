#ifndef NET_RPC_SERVER_H
#define NET_RPC_SERVER_H

namespace Hainan {

class RpcServer: public boost::enable_shared_from_this<RpcServer>
{
private:
	friend class RpcSession;
	typedef boost::unordered_set<RpcSessionPtr> RpcSessionSet;
	google::protobuf::Service& service;
	boost::asio::io_service io_service;
	boost::asio::ip::tcp::acceptor acceptor;
	ThreadPool thread_pool;
	RpcSessionSet sessions;

	void RunService(boost::asio::ip::tcp::socket& socket, RpcRequestPtr request);

public:
	RpcServer(google::protobuf::Service& pservice, int port);
	~RpcServer();
	void Start();
	void Stop();
};

} // namespace Hainan

#endif // NET_RPC_SERVER_H
