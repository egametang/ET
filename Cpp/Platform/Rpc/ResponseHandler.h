#ifndef RPC_RESPONSEHANDLER_H
#define RPC_RESPONSEHANDLER_H

#include <boost/function.hpp>
#include <google/protobuf/service.h>
#include <google/protobuf/message.h>
#include "Base/Typedef.h"
#include "Rpc/RpcTypedef.h"

namespace Egametang {

class ResponseHandler
{
private:
	const google::protobuf::MethodDescriptor* method;
	google::protobuf::Message* request;
	google::protobuf::Message* response;
	std::size_t id;
	MessageHandler messageHandler;

public:
	ResponseHandler(MethodInfoPtr& method_info, std::size_t id, MessageHandler& sendMessage);

	~ResponseHandler();

	const google::protobuf::MethodDescriptor* Method();

	google::protobuf::Message* Request();

	google::protobuf::Message* Response();

	void Run();
};

} // namespace Egametang

#endif // RPC_RESPONSEHANDLER_H
