#ifndef RPC_TYPEDEF_H
#define RPC_TYPEDEF_H

#include <boost/bind.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/function.hpp>
#include "Base/Typedef.h"

namespace Egametang {
// rpc
class RpcServer;
class RpcSession;
class RpcClient;
class RequestHandler;
class MethodInfo;
class RpcMeta;
class ResponseHandler;

typedef boost::shared_ptr<RpcServer> 	            RpcServerPtr;
typedef boost::shared_ptr<RpcSession>               RpcSessionPtr;
typedef boost::shared_ptr<RpcClient>                RpcClientPtr;
typedef boost::shared_ptr<MethodInfo>               MethodInfoPtr;
typedef boost::shared_ptr<RpcMeta> 	                RpcMetaPtr;
typedef boost::shared_ptr<RequestHandler>           RequestHandlerPtr;
typedef boost::shared_ptr<ResponseHandler>          ResponseHandlerPtr;

typedef boost::function<void (RpcMetaPtr, StringPtr)> MessageHandler;

} // namespace Egametang

#endif // RPC_TYPEDEF_H
