#include "Net/RpcHandler.h"

namespace Hainan {

RpcHandler::RpcHandler(google::protobuf::RpcController* p_controller,
		google::protobuf::Message* p_response,
		google::protobuf::Closure* p_done):
		controller(p_controller), response(p_response), done(p_done)
{
}

google::protobuf::RpcController *RpcHandler::GetController() const
{
    return controller;
}

google::protobuf::Closure *RpcHandler::GetDone() const
{
    return done;
}

google::protobuf::Message *RpcHandler::GetResponse() const
{
    return response;
}

} // namespace Hainan
