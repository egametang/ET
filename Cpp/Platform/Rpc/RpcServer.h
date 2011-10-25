#ifndef RPC_RPCSERVER_H
#define RPC_RPCSERVER_H
#include <boost/asio.hpp>
#include <boost/function.hpp>
#include <boost/unordered_set.hpp>
#include <boost/unordered_map.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <google/protobuf/service.h>
#include "Base/Marcos.h"
#include "Thread/ThreadPool.h"
#include "Rpc/RpcTypedef.h"

namespace Egametang {

class RpcServer: public boost::noncopyable, public boost::enable_shared_from_this<RpcServer>
{
private:
	typedef boost::unordered_set<RpcSessionPtr> RpcSessionSet;
	typedef boost::unordered_map<std::size_t, MethodInfoPtr> MethodMap;

	boost::asio::io_service& ioService;
	boost::asio::ip::tcp::acceptor acceptor;
	ThreadPool threadPool;
	RpcSessionSet sessions;
	MethodMap methods;

	void OnAsyncAccept(RpcSessionPtr session, const boost::system::error_code& err);
	void OnCallMethod(RpcSessionPtr session, ResponseHandlerPtr responseHandler);

public:
	RpcServer(boost::asio::io_service& service, int port);
	virtual ~RpcServer();

	virtual void RunService(RpcSessionPtr session, RpcMetaPtr meta,
			StringPtr message, MessageHandler messageHandler);
	virtual void Register(RpcServicePtr service);
	virtual void Remove(RpcSessionPtr& session);
	virtual void Stop();
};

} // namespace Egametang

#endif // RPC_RPCSERVER_H
