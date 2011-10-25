#ifndef RPC_RPCCLIENT_H
#define RPC_RPCCLIENT_H

#include <boost/unordered_map.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/asio.hpp>
#include <boost/thread.hpp>
#include <google/protobuf/service.h>
#include "Base/Typedef.h"
#include "Rpc/RpcTypedef.h"
#include "Rpc/RpcCommunicator.h"

namespace Egametang {

class RpcHandler;

class RpcClient:
	public google::protobuf::RpcChannel, public RpcCommunicator,
	public boost::enable_shared_from_this<RpcClient>
{
private:
	typedef boost::unordered_map<std::size_t, RequestHandlerPtr> RequestHandlerMap;

	std::size_t id;
	RequestHandlerMap requestHandlers;

	void OnAsyncConnect(const boost::system::error_code& err);

	virtual void OnRecvMessage(RpcMetaPtr meta, StringPtr message);
	virtual void OnSendMessage(RpcMetaPtr meta, StringPtr message);

public:
	RpcClient(boost::asio::io_service& service, std::string host, int port);
	~RpcClient();
	virtual void CallMethod(
			const google::protobuf::MethodDescriptor* method,
			google::protobuf::RpcController* controller,
			const google::protobuf::Message* request,
			google::protobuf::Message* response,
			google::protobuf::Closure* done);
};

} // namespace Egametang

#endif // RPC_RPCCLIENT_H
