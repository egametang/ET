#include "Rpc/RequestHandler.h"

namespace Egametang {

RequestHandler::RequestHandler(
		google::protobuf::Message* response, google::protobuf::Closure* done):
		response(response), done(done)
{
}

google::protobuf::Message *RequestHandler::Response() const
{
	return response;
}

void RequestHandler::Run()
{
	if (done)
	{
		done->Run();
	}
}

} // namespace Egametang
