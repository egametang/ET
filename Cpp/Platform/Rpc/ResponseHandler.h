#ifndef RPC_RESPONSEHANDLER_H
#define RPC_RESPONSEHANDLER_H

#include <boost/function.hpp>
#include <google/protobuf/service.h>
#include <google/protobuf/message.h>
#include "Base/Typedef.h"
#include "Rpc/Typedef.h"

namespace Egametang {

class ResponseHandler
{
private:
	const google::protobuf::MethodDescriptor& method;
	google::protobuf::Message* request;
	google::protobuf::Message* response;
	std::size_t id;
	MessageHandler messageHandler;

public:
	ResponseHandler(const RpcMetaPtr meta, const StringPtr message,
			MethodInfoPtr& methodInfo, MessageHandler& messageHandler);

	~ResponseHandler();

	const google::protobuf::MethodDescriptor& Method() const;

	const google::protobuf::Message* Request() const;

	google::protobuf::Message* Response();

	void Run();
};

} // namespace Egametang

#endif // RPC_RESPONSEHANDLER_H
