#ifndef NET_RPC_HANDLER_H
#define NET_RPC_HANDLER_H

#include "boost/shared_ptr.hpp"

namespace Egametang {

class google::protobuf::RpcController;
class google::protobuf::Message;
class google::protobuf::Closure;

class RpcHandler
{
private:
	google::protobuf::RpcController* controller_;
	google::protobuf::Message* response_;
	google::protobuf::Closure* done_;
public:
	RpcHandler(google::protobuf::RpcController* controller,
			google::protobuf::Message* response,
			google::protobuf::Closure* done);
    google::protobuf::RpcController *GetController() const;
    google::protobuf::Closure *GetDone() const;
    google::protobuf::Message *GetResponse() const;
};

typedef boost::shared_ptr<RpcHandler> RpcHandlerPtr;

} // namespace Egametang

#endif // NET_RPC_HANDLER_H
