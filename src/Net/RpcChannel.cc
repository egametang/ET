#include <boost/asio.hpp>
#include <boost/make_shared.hpp>
#include <google/protobuf/message.h>
#include "Net/RpcChannel.h"
#include "Net/RpcCommunicator.h"

namespace Hainan {

RpcChannel::RpcChannel(std::string& host, int port):
		id(0), communicator(host, port)
{
	RecvResponse();
}

void RpcChannel::RecvResponseHandler(IOStreamPtr input, RpcRequestPtr request,
		const boost::system::error_code& err)
{
	if (err)
	{
		LOG(FATAL) << "receive response failed";
	}
	int32 id = request->id();
	RpcHandlerPtr handler = handlers[id];
	handler->GetResponse()->ParsePartialFromIstream(input.get());
	RecvResponse();
}

void RpcChannel::RecvResponse()
{
	std::stringstream ss;
	communicator.AsyncRead(boost::asio::buffer(ss),
			boost::bind(&RpcChannel::RecvResponseHandler, this,
					boost::asio::placeholders::error));
}

void RpcChannel::SendRequestHandler(int32 id, RpcHandlerPtr handler,
		const boost::asio::error_code err)
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

void RpcChannel::SendRequestHandler(int32 id, RpcHandlerPtr handler,
		const boost::asio::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "SendRequestHandler error:" << e.what();
		return;
	}
}

void RpcChannel::SendSizeHandler(int32 id, RpcHandlerPtr handler,
		const boost::asio::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "SendSizeHandler error:" << e.what();
		return;
	}
	string ss = request.SerializeAsString();
	boost::asio::async_write(boost::asio::buffer(ss),
			boost::bind(&RpcChannel::SendRequestHandler, this, request.id(),
					handler, boost::asio::placeholders::error));
}

void RpcChannel::SendMessage(const RpcRequestPtr request, RpcHandlerPtr handler)
{
	int size = request->ByteSize();
	string ss = boost::lexical_cast(size);
	boost::asio::async_write(boost::asio::buffer(ss),
			boost::bind(&RpcChannel::SendSizeHandler, this, request->id(),
					handler, boost::asio::placeholders::error));
}

void RpcChannel::CallMethod(
		const google::protobuf::MethodDescriptor* method,
		google::protobuf::RpcController* controller,
		const google::protobuf::Message* request,
		google::protobuf::Message* response,
		google::protobuf::Closure* done)
{
	RpcRequestPtr req(new RpcRequest);
	req->set_id(++id);
	req->set_method(method->full_name());
	req->set_request(request->SerializeAsString());
	RpcHandlerPtr handler(new RpcHandler(controller, response, done));
	SendMessage(req, handler);
}

} // namespace Hainan
