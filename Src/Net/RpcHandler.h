#ifndef NET_RPC_HANDLER_H
#define NET_RPC_HANDLER_H

#include "boost/shared_ptr.hpp"

namespace Hainan {

class google::protobuf::RpcController;
class google::protobuf::Message;
class google::protobuf::Closure;

class RpcHandler
{
private:
	google::protobuf::RpcController* controller;
	google::protobuf::Message* response;
	google::protobuf::Closure* done;
public:
	RpcHandler(google::protobuf::RpcController* p_controller,
			google::protobuf::Message* p_response,
			google::protobuf::Closure* p_done);
    google::protobuf::RpcController *GetController() const;
    google::protobuf::Closure *GetDone() const;
    google::protobuf::Message *GetResponse() const;
};

typedef boost::shared_ptr<RpcHandler> RpcHandlerPtr;

} // namespace Hainan

#endif // NET_RPC_HANDLER_H
