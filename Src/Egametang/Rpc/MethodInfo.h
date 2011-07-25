#ifndef RPC_METHODINFO_H
#define RPC_METHODINFO_H

#include <google/protobuf/service.h>
#include <google/protobuf/message.h>
#include "Rpc/RpcTypedef.h"

namespace Egametang {

struct MethodInfo
{
	RpcServicePtr service;
	const google::protobuf::MethodDescriptor* method_descriptor;
	const google::protobuf::Message* request_prototype;
	const google::protobuf::Message* response_prototype;

	MethodInfo(RpcServicePtr service, const google::protobuf::MethodDescriptor* method_descriptor):
		service(service), method_descriptor(method_descriptor)
	{
		request_prototype = &service->GetRequestPrototype(method_descriptor);
		response_prototype = &service->GetResponsePrototype(method_descriptor);
	}
};

} // namespace Egametang

#endif // RPC_METHODINFO_H
