#include "Rpc/RpcHandler.h"

namespace Egametang {

RpcHandler::RpcHandler(google::protobuf::RpcController* controller,
		google::protobuf::Message* response,
		google::protobuf::Closure* done):
		controller_(controller), response_(response), done_(done)
{
}

google::protobuf::RpcController *RpcHandler::GetController() const
{
    return controller_;
}

google::protobuf::Closure *RpcHandler::GetDone() const
{
    return done_;
}

google::protobuf::Message *RpcHandler::GetResponse() const
{
    return response_;
}

} // namespace Egametang
