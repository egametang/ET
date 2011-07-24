#ifndef RPC_RPCSERVER_H
#define RPC_RPCSERVER_H
#include <boost/asio.hpp>
#include <boost/function.hpp>
#include "Rpc/RpcTypedef.h"

namespace Egametang {

class RpcServer: public google::protobuf::Service, boost::enable_shared_from_this<RpcServer>
{
private:
	typedef boost::unordered_set<RpcSessionPtr> RpcSessionSet;
	typedef boost::unordered_map<std::size_t, MethodInfoPtr> MethodMap;

	boost::asio::io_service& io_service;
	boost::asio::ip::tcp::acceptor acceptor;
	ThreadPool thread_pool;
	RpcSessionSet sessions;
	MethodMap methods;

	void OnAsyncAccept(RpcSessionPtr session, const boost::system::error_code& err);
	void OnCallMethod(RpcSessionPtr session, ResponseHandlerPtr response_handler);

public:
	RpcServer(boost::asio::io_service& io_service, int port);
	virtual ~RpcServer();

	boost::asio::io_service& IOService();
	void RunService(RpcSessionPtr session, RpcMetaPtr meta,
			StringPtr message, MessageHandler handler);
	void RegisterService(RpcServicePtr service);
	void RemoveSession(RpcSessionPtr& session);
	void Stop();
};

} // namespace Egametang

#endif // RPC_RPCSERVER_H
