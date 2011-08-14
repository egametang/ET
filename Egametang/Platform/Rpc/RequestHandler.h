#ifndef RPC_REQUESTHANDLER_H
#define RPC_REQUESTHANDLER_H

#include <google/protobuf/service.h>
#include <google/protobuf/message.h>

namespace Egametang {

class RequestHandler
{
private:
	google::protobuf::Message* response;
	google::protobuf::Closure* done;

public:
	RequestHandler(
			google::protobuf::Message* response,
			google::protobuf::Closure* done);

    google::protobuf::Message *Response() const;

    void Run();
};

} // namespace Egametang

#endif // RPC_REQUESTHANDLER_H
