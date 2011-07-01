#ifndef RPC_RPCCHANNEL_H
#define RPC_RPCCHANNEL_H

#include <google/protobuf/service.h>
#include <boost/unordered_map.hpp>
#include <boost/asio.hpp>
#include "Base/Typedef.h"
#include "Rpc/RpcTypedef.h"
#include "Rpc/RpcCommunicator.h"

namespace Egametang {

class RpcHandler;

class RpcChannel:
		public google::protobuf::RpcChannel,
		public RpcCommunicator
{
private:
	typedef boost::unordered_map<int32, RpcHandlerPtr> RpcCallbackMap;

	int32 id_;
	RpcCallbackMap handlers_;

	void OnAsyncConnect(const boost::system::error_code& err);
	void SendRequest(RpcRequestPtr request);

	virtual void OnRecvMessage(StringPtr ss);
	virtual void OnSendMessage();

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
