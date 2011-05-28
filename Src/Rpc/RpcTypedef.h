#ifndef RPC_RPC_TYPEDEF_H
#define RPC_RPC_TYPEDEF_H
#include <boost/shared_ptr.hpp>
#include <google/protobuf/service.h>

namespace Egametang {

// google
typedef boost::shared_ptr<google::protobuf::Service> ProtobufServicePtr;
typedef boost::shared_ptr<google::protobuf::Message> ProtobufMessagePtr;

// rpc
class RpcSession;
class RpcRequest;
class RpcChannel;
class RpcHandler;
class RpcResponse;
typedef boost::shared_ptr<RpcSession> RpcSessionPtr;
typedef boost::shared_ptr<RpcRequest> RpcRequestPtr;
typedef boost::shared_ptr<RpcChannel> RpcChannelPtr;
typedef boost::shared_ptr<RpcHandler> RpcHandlerPtr;
typedef boost::shared_ptr<RpcResponse> RpcResponsePtr;

} // namespace Egametang

#endif // RPC_RPC_TYPEDEF_H
