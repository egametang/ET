#include <google/protobuf/message.h>
#include "net/rpc_channel.h"

namespace hainan {

google::protobuf::Closure* to_closure();

rpc_channel::rpc_channel(std::string& host, int port):
		id(0)
{
}

//void client_channel::register_service(const google::protobuf::Service service) {
//	const google::protobuf::ServiceDescriptor* service_descriptor =
//			service->GetDescriptor();
//	for (int i = 0; i < service_descriptor; ++i)
//	{
//		const google::protobuf::MethodDescriptor* method =
//				service_descriptor->method(i);
//		std::string method_name(method->full_name());
//		service_map_[method_name] = service;
//	}
//}

void rpc_channel::CallMethod(
		const google::protobuf::MethodDescriptor* method,
		google::protobuf::RpcController* controller,
		const google::protobuf::Message* request,
		google::protobuf::Message* response,
		google::protobuf::Closure* done) {
	rpc_request req;
	req.set_id(++id);
	req.set_method(method->full_name());
	req.set_request(request->SerializeAsString());
	rpc_callback callback(controller, response, done);
	communicator.send_message(req, callback);
}

} // namespace hainan
