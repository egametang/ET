#include <google/protobuf/service.h>
#include <google/protobuf/message.h>
#include "net/rpc_callback.h"

namespace hainan {

rpc_callback::rpc_callback(google::protobuf::RpcController* controller,
		google::protobuf::Message* response,
		google::protobuf::Closure* done):
		controller_(controller), response_(response), done_(done)
{
}

google::protobuf::RpcController* rpc_callback::controller() const
{
    return controller_;
}

google::protobuf::Closure* rpc_callback::done() const
{
    return done_;
}

google::protobuf::Message* rpc_callback::response() const
{
    return response_;
}

} // hainan
