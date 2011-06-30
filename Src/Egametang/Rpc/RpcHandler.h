#ifndef RPC_RPCHANDLER_H
#define RPC_RPCHANDLER_H

#include <google/protobuf/service.h>
#include <google/protobuf/message.h>

namespace Egametang {

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

} // namespace Egametang

#endif // RPC_RPCHANDLER_H
