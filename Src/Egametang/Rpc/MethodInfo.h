#ifndef RPC_METHODINFO_H
#define RPC_METHODINFO_H

#include <google/protobuf/service.h>
#include <google/protobuf/message.h>

namespace Egametang {

struct MethodInfo
{
	RpcServicePtr service;
	const google::protobuf::MethodDescriptor* method_descriptor;
	google::protobuf::Message* request_prototype;
	google::protobuf::Message* response_prototype;

	MethodInfo(RpcServicePtr service, const google::protobuf::MethodDescriptor* method_descriptor):
		service(service), method_descriptor(method_descriptor)
	{
		request_prototype = &service->GetRequestPrototype(method_descriptor);
		response_prototype = &service->GetResponsePrototype(method_descriptor);
	}
};

} // namespace Egametang

#endif // RPC_METHODINFO_H
