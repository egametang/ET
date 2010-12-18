#ifndef NET_RPC_SERVER_H
#define NET_RPC_SERVER_H

namespace Hainan {

class RpcServer
{
private:
	typedef boost::unordered_set<RpcSessionPtr> RpcSessionSet;

	google::protobuf::Service& service;
	boost::asio::io_service io_service;
	boost::asio::ip::tcp::acceptor acceptor;
	RpcSessionSet sessions;

public:
	RpcServer(google::protobuf::Service& pservice, int port);
	~RpcServer();
	void Start();
	void Stop();
};

} // namespace Hainan

#endif // NET_RPC_SERVER_H
