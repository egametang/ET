#ifndef RPC_RPCCHANNEL_H
#define RPC_RPCCHANNEL_H

#include <google/protobuf/service.h>
#include <boost/unordered_map.hpp>
#include <boost/asio.hpp>
#include <boost/thread.hpp>
#include "Base/Typedef.h"
#include "Rpc/RpcTypedef.h"
#include "Rpc/RpcCommunicator.h"

namespace Egametang {

class RpcHandler;

class RpcChannel:
	public google::protobuf::RpcChannel, public RpcCommunicator,
	public boost::enable_shared_from_this<RpcChannel>
{
private:
	typedef boost::unordered_map<std::size_t, RequestHandlerPtr> RequestHandlerMap;

	std::size_t id;
	RequestHandlerMap request_handlers;

	void OnAsyncConnect(const boost::system::error_code& err);

	virtual void OnRecvMessage(RpcMetaPtr meta, StringPtr message);
	virtual void OnSendMessage(RpcMetaPtr meta, StringPtr message);

	void HandleStop();

public:
	RpcChannel(boost::asio::io_service& service, std::string host, int port);
	~RpcChannel();
	virtual void Stop();
	virtual void CallMethod(
			const google::protobuf::MethodDescriptor* method,
			google::protobuf::RpcController* controller,
			const google::protobuf::Message* request,
			google::protobuf::Message* response,
			google::protobuf::Closure* done);
};

} // namespace Egametang

#endif // RPC_RPCCHANNEL_H
