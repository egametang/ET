#ifndef RPC_METHODINFO_H
#define RPC_METHODINFO_H

#include <google/protobuf/service.h>
#include <google/protobuf/message.h>
#include "Rpc/Typedef.h"

namespace Egametang {

class MethodInfo
{
private:
	ProtobufServicePtr service;
	const google::protobuf::MethodDescriptor* methodDescriptor;

public:
	MethodInfo(ProtobufServicePtr service, const google::protobuf::MethodDescriptor* methodDescriptor);
	ProtobufServicePtr GetService();
	const google::protobuf::MethodDescriptor& GetMethodDescriptor() const;
	const google::protobuf::Message& GetRequestPrototype() const;
	const google::protobuf::Message& GetResponsePrototype() const;
};

} // namespace Egametang

#endif // RPC_METHODINFO_H
