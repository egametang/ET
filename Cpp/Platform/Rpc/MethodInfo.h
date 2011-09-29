#ifndef RPC_METHODINFO_H
#define RPC_METHODINFO_H

#include <google/protobuf/service.h>
#include <google/protobuf/message.h>
#include "Rpc/RpcTypedef.h"

namespace Egametang {

struct MethodInfo
{
	RpcServicePtr service;
	const google::protobuf::MethodDescriptor* methodDescriptor;
	const google::protobuf::Message* requestPrototype;
	const google::protobuf::Message* responsePrototype;

	MethodInfo(RpcServicePtr service, const google::protobuf::MethodDescriptor* method_descriptor):
		service(service), methodDescriptor(method_descriptor)
	{
		requestPrototype = &service->GetRequestPrototype(method_descriptor);
		responsePrototype = &service->GetResponsePrototype(method_descriptor);
	}
};

} // namespace Egametang

#endif // RPC_METHODINFO_H
