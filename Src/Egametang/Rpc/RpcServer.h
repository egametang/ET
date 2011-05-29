#ifndef RPC_RPC_SERVER_H
#define RPC_RPC_SERVER_H
#include <boost/asio.hpp>
#include "Rpc/RpcTypedef.h"

namespace Egametang {

class RpcServer: public boost::enable_shared_from_this<RpcServer>
{
private:
	typedef boost::unordered_set<RpcSessionPtr> RpcSessionSet;

	google::protobuf::Service& service_;
	boost::asio::io_service& io_service_;
	boost::asio::ip::tcp::acceptor acceptor_;
	ThreadPool& thread_pool_;
	RpcSessionSet sessions_;

	void HandleAsyncAccept(RpcSessionPtr session, const boost::system::error_code& err);
	void Callback(RpcSessionPtr session,
			boost::function<void (RpcSessionPtr, RpcResponsePtr)> handler);

public:
	RpcServer(boost::asio::io_service& io_service, int port, ThreadPool& thread_pool);
	~RpcServer();
	void Start();
	void Stop();

	void RunService(RpcSessionPtr session, RpcRequestPtr request,
			boost::function<void (RpcSessionPtr, RpcResponsePtr)> handler);
	void RegisterService(ProtobufServicePtr service);
};

} // namespace Egametang

#endif // RPC_RPC_SERVER_H
