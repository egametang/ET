#include <boost/asio.hpp>
#include <boost/make_shared.hpp>
#include <google/protobuf/message.h>
#include "Net/RpcChannel.h"
#include "Net/RpcCommunicator.h"

namespace Hainan {

RpcChannel::RpcChannel(std::string& host, int port):
		id(0)
{
	// socket.async_connect(endpoint, );
}

void RpcCommunicator::SendRequestHandler(int32 id, RpcHandlerPtr handler,
		const boost::system::error_code& err)
{
	if (err)
	{
		handler->GetController()->SetFailed("failed");
	}
	else
	{
		handlers[id] = handler;
	}
}

void RpcChannel::SendRequest(const RpcRequest& request, RpcHandlerPtr handler)
{
	int size = request.ByteSize();
	std::stringstream ss;
	ss << size;
	ss << request.SerializeAsString();
	communicator.AsyncWrite(boost::asio::buffer(ss),
			boost::bind(&RpcChannel::SendRequestHandler, this, request.id(),
					handler, boost::asio::placeholders::error));
}

void RpcChannel::CallMethod(
		const google::protobuf::MethodDescriptor* method,
		google::protobuf::RpcController* controller,
		const google::protobuf::Message* request,
		google::protobuf::Message* response,
		google::protobuf::Closure* done) {
	RpcRequest req;
	req.set_id(++id);
	req.set_method(method->full_name());
	req.set_request(request->SerializeAsString());
	RpcHandlerPtr handler = boost::make_shared<RpcHandler>(
			controller, response, done);
	SendRequest(req, handler);
}

} // namespace Hainan
