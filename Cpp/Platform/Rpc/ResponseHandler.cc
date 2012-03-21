#include <boost/make_shared.hpp>
#include "Rpc/MethodInfo.h"
#include "Rpc/ResponseHandler.h"
#include "Rpc/RpcCommunicator.h"

namespace Egametang {

ResponseHandler::ResponseHandler(
		MethodInfoPtr& methodInfo, std::size_t id, MessageHandler& messageHandler):
		id(id), messageHandler(messageHandler)
{
	method = methodInfo->methodDescriptor;
	request = methodInfo->requestPrototype->New();
	response = methodInfo->responsePrototype->New();
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
	auto meta = boost::make_shared<RpcMeta>();
	auto message = boost::make_shared<std::string>();
	response->SerializeToString(message.get());
	meta->id = id;
	meta->size = message->size();
	messageHandler(meta, message);
}

} // namespace Egametang
