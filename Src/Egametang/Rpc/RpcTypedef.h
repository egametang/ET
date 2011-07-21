#ifndef RPC_RPC_TYPEDEF_H
#define RPC_RPC_TYPEDEF_H
#include <boost/shared_ptr.hpp>
#include <google/protobuf/service.h>

namespace Egametang {

// google
typedef boost::shared_ptr<google::protobuf::Service> RpcServicePtr;
typedef boost::shared_ptr<google::protobuf::Message> RpcMessagePtr;

// rpc
class RpcServer;
class RpcSession;
class RpcChannel;
class RpcHandler;
class MethodInfo;
class RpcMeta;
class CallMethodBack;

typedef boost::shared_ptr<RpcServer> 	    RpcServerPtr;
typedef boost::shared_ptr<RpcSession>       RpcSessionPtr;
typedef boost::shared_ptr<RpcChannel>       RpcChannelPtr;
typedef boost::shared_ptr<RpcHandler>       RpcHandlerPtr;
typedef boost::shared_ptr<MethodInfo>       MethodInfoPtr;
typedef boost::shared_ptr<RpcMeta> 	        RpcMetaPtr;
typedef boost::shared_ptr<CallMethodBack> CallMethodBackPtr;


typedef boost::weak_ptr<RpcServer>     RpcServerWPtr;

typedef boost::function<void (std::size_t, google::protobuf::Message*)> SendResponseHandler;

} // namespace Egametang

#endif // RPC_RPC_TYPEDEF_H
