#ifndef RPC_TYPEDEF_H
#define RPC_TYPEDEF_H

#include <memory>
#include <functional>
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

typedef std::shared_ptr<RpcServer> 	                RpcServerPtr;
typedef std::shared_ptr<RpcSession>                 RpcSessionPtr;
typedef std::shared_ptr<RpcClient>                  RpcClientPtr;
typedef std::shared_ptr<MethodInfo>                 MethodInfoPtr;
typedef std::shared_ptr<RpcMeta> 	                RpcMetaPtr;
typedef std::shared_ptr<RequestHandler>             RequestHandlerPtr;
typedef std::shared_ptr<ResponseHandler>            ResponseHandlerPtr;

typedef std::function<void (RpcMetaPtr, StringPtr)> MessageHandler;

} // namespace Egametang

#endif // RPC_TYPEDEF_H
