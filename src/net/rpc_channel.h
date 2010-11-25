#ifndef NET_RPC_CHANNEL_H
#define NET_RPC_CHANNEL_H

#include <google/protobuf/service.h>
#include <boost/unordered_map.hpp>
#include <boost/asio.hpp>
#include "base/base.h"
#include "net/rpc_communicator.h"

namespace hainan {

class rpc_channel: public google::protobuf::RpcChannel
{
private:
	int32 id;
	rpc_communicator communicator;
public:
	rpc_channel(std::string& host, int port);
	~rpc_channel();
	virtual void CallMethod(
			const google::protobuf::MethodDescriptor* method,
			google::protobuf::RpcController* controller,
			const google::protobuf::Message* request,
			google::protobuf::Message* response,
			google::protobuf::Closure* done);
};

} // namespace hainan

#endif // NET_RPC_CHANNEL_H
