#include <boost/make_shared.hpp>
#include <glog/logging.h>
#include "Rpc/MethodInfo.h"
#include "Rpc/ResponseHandler.h"
#include "Rpc/RpcCommunicator.h"

namespace Egametang {

ResponseHandler::ResponseHandler(
		const RpcMetaPtr meta, const StringPtr message,
		MethodInfoPtr& methodInfo, MessageHandler& messageHandler):
		method(methodInfo->GetMethodDescriptor()),
		id(meta->id), messageHandler(messageHandler)
{
	request = methodInfo->GetRequestPrototype().New();
	response = methodInfo->GetResponsePrototype().New();
	request->ParseFromString(*message);
}

ResponseHandler::~ResponseHandler()
{
	delete request;
	delete response;
}

const google::protobuf::MethodDescriptor& ResponseHandler::Method() const
{
	return method;
}

const google::protobuf::Message* ResponseHandler::Request() const
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
