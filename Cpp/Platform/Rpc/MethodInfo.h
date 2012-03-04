#ifndef RPC_METHODINFO_H
#define RPC_METHODINFO_H

#include <google/protobuf/service.h>
#include <google/protobuf/message.h>
#include "Rpc/Typedef.h"

namespace Egametang {

class MethodInfo
{
public:
	ProtobufServicePtr service;
	const google::protobuf::MethodDescriptor* methodDescriptor;
	const google::protobuf::Message* requestPrototype;
	const google::protobuf::Message* responsePrototype;

	MethodInfo(ProtobufServicePtr service, const google::protobuf::MethodDescriptor* methodDescriptor):
		service(service), methodDescriptor(methodDescriptor)
	{
		requestPrototype = &service->GetRequestPrototype(methodDescriptor);
		responsePrototype = &service->GetResponsePrototype(methodDescriptor);
	}
};

} // namespace Egametang

#endif // RPC_METHODINFO_H
