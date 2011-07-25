#include "Rpc/MethodInfo.h"
#include "Rpc/ResponseHandler.h"
#include "Rpc/RpcCommunicator.h"

namespace Egametang {

ResponseHandler::ResponseHandler(
		MethodInfoPtr& method_info, std::size_t id, MessageHandler& send_message):
		id(id), message_handler(message_handler)
{
	method = method_info->method_descriptor;
	request = method_info->request_prototype->New();
	response = method_info->response_prototype->New();
}

ResponseHandler::~ResponseHandler()
{
	delete request;
	delete response;
}

const google::protobuf::MethodDescriptor* ResponseHandler::Method()
{
	return method;
}

google::protobuf::Message* ResponseHandler::Request()
{
	return request;
}

google::protobuf::Message* ResponseHandler::Response()
{
	return response;
}

void ResponseHandler::Run()
{
	RpcMetaPtr meta(new RpcMeta());
	StringPtr message(new std::string);
	response->SerializeToString(message.get());
	meta->id = id;
	meta->size = message->size();
	message_handler(meta, message);
}

} // namespace Egametang
