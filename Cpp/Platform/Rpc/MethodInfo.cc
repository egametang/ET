// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include "Rpc/MethodInfo.h"

namespace Egametang {

MethodInfo::MethodInfo(
		ProtobufServicePtr service, const google::protobuf::MethodDescriptor* methodDescriptor):
	service(service), methodDescriptor(methodDescriptor)
{
}

ProtobufServicePtr MethodInfo::GetService()
{
	return service;
}

const google::protobuf::MethodDescriptor& MethodInfo::GetMethodDescriptor() const
{
	return *methodDescriptor;
}

const google::protobuf::Message& MethodInfo::GetRequestPrototype() const
{
	return service->GetRequestPrototype(methodDescriptor);
}


const google::protobuf::Message& MethodInfo::GetResponsePrototype() const
{
	return service->GetResponsePrototype(methodDescriptor);
}

} // Egametang
