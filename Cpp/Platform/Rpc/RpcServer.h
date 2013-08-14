#ifndef RPC_RPCSERVER_H
#define RPC_RPCSERVER_H

#include <boost/bind.hpp>
#include <unordered_set>
#include <unordered_map>
#include <boost/asio.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/threadpool.hpp>
#include <google/protobuf/service.h>
#include "Base/Marcos.h"
#include "Rpc/Typedef.h"

namespace Egametang {

typedef std::unordered_set<RpcSessionPtr> RpcSessionSet;
typedef std::unordered_map<std::size_t, MethodInfoPtr> MethodMap;

class RpcServer: public boost::noncopyable, public boost::enable_shared_from_this<RpcServer>
{
private:
	friend class RpcServerTest;

	boost::asio::io_service& ioService;
	boost::asio::ip::tcp::acceptor acceptor;
	boost::threadpool::fifo_pool threadPool;
	RpcSessionSet sessions;
	MethodMap methods;

	void OnAsyncAccept(RpcSessionPtr session, const boost::system::error_code& err);
	void OnCallMethod(RpcSessionPtr session, ResponseHandlerPtr responseHandler);

public:
	RpcServer(boost::asio::io_service& service, int port);
	virtual ~RpcServer();

	virtual void RunService(
			RpcSessionPtr session, const RpcMetaPtr meta,
			const StringPtr message, MessageHandler messageHandler);
	virtual void Register(ProtobufServicePtr service);
	virtual void Remove(RpcSessionPtr session);
};

} // namespace Egametang

#endif // RPC_RPCSERVER_H
