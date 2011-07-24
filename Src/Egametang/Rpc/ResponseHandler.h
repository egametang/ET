#ifndef RPC_RESPONSEHANDLER_H
#define RPC_RESPONSEHANDLER_H

#include <google/protobuf/service.h>
#include <google/protobuf/message.h>

namespace Egametang {

class ResponseHandler
{
private:
	google::protobuf::MethodDescriptor* method;
	google::protobuf::Message* request;
	google::protobuf::Message* response;
	std::size_t id;
	MessageHandler message_handler;

public:
	ResponseHandler(MethodInfoPtr& method_info, std::size_t id, MessageHandler& send_message);

	~ResponseHandler();

	google::protobuf::MethodDescriptor* Method();

	google::protobuf::Message* Request();

	google::protobuf::Message* Response();

	void Run();
};

} // namespace Egametang

#endif // RPC_RESPONSEHANDLER_H
