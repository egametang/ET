#ifndef NET_RPC_CHANNEL_H
#define NET_RPC_CHANNEL_H

#include <google/protobuf/service.h>
#include <boost/unordered_map.hpp>
#include <boost/asio.hpp>
#include "Base/Base.h"
#include "Net/RpcCommunicator.h"

namespace Hainan {

class RpcHandler;

class RpcChannel: public google::protobuf::RpcChannel
{
	typedef boost::unordered_map<int32, RpcHandlerPtr> RpcCallbackMap;
private:
	int32 id;
	RpcCallbackMap handlers;
	RpcCommunicator communicator;

	void SendRequestHandler(int32 id, RpcHandlerPtr handler,
			const boost::system::error_code& err);
	void SendRequest(const RpcRequest& request, RpcHandlerPtr handler);
public:
	RpcChannel(std::string& host, int port);
	~RpcChannel();
	virtual void CallMethod(
			const google::protobuf::MethodDescriptor* method,
			google::protobuf::RpcController* controller,
			const google::protobuf::Message* request,
			google::protobuf::Message* response,
			google::protobuf::Closure* done);
};

} // namespace Hainan

#endif // NET_RPC_CHANNEL_H
