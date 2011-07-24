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

	boost::asio::io_service& io_service_;
	boost::asio::ip::tcp::acceptor acceptor_;
	ThreadPool thread_pool_;
	RpcSessionSet sessions_;
	MethodMap methods_;

	void OnAsyncAccept(RpcSessionPtr session, const boost::system::error_code& err);
	void OnCallMethod(RpcSessionPtr session, ResponseHandlerPtr call_method_back);

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
