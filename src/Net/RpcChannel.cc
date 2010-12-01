#include <boost/asio.hpp>
#include <boost/make_shared.hpp>
#include <google/protobuf/message.h>
#include "Net/RpcChannel.h"
#include "Net/RpcCommunicator.h"

namespace Hainan {

RpcChannel::RpcChannel(std::string& host, int port):
		id(0), communicator(host, port)
{
	// another thread?
	RecvMessage();
}

void RpcChannel::RecvResponseHandler(StringPtr ss,
		const boost::asio::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "receive response failed";
		return;
	}

	RpcResponse response;
	Response->ParseFromString(*ss);
	RpcHandlerPtr handler = handlers[response.id()];
	handler->GetResponse()->ParseFromString(response.response());
	handlers.erase(response.id());

	RecvMessage();
}

void RpcChannel::RecvSizeHandler(IntPtr size,
		const boost::asio::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "receive response size failed";
		return;
	}
	StringPtr ss;
	boost::asio::async_read(socket,
			boost::asio::buffer(*ss, *size),
			boost::bind(&RpcChannel::RecvResponseHandler, this, ss,
					boost::asio::placeholders::error));
}

void RpcChannel::RecvMessage()
{
	IntPtr size(new int);
	boost::asio::async_read(socket,
			boost::asio::buffer(
					reinterpret_cast<char*>(size.get()), sizeof(int)),
			boost::bind(&RpcChannel::RecvSizeHandler, this, size,
					boost::asio::placeholders::error));
}

void RpcChannel::SendRequestHandler(int32 id, RpcHandlerPtr handler,
		const boost::asio::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "SendRequestHandler error:" << e.what();
		return;
	}
	handlers[id] = handler;
}

void RpcChannel::SendSizeHandler(const RpcRequestPtr request,
		RpcHandlerPtr handler, const boost::asio::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "SendSizeHandler error:" << e.what();
		return;
	}
	std::string ss = request->SerializeAsString();
	boost::asio::async_write(socket, boost::asio::buffer(ss),
			boost::bind(&RpcChannel::SendRequestHandler, this, request->id(),
					handler, boost::asio::placeholders::error));
}

void RpcChannel::SendMessage(const RpcRequestPtr request, RpcHandlerPtr handler)
{
	int size = request->ByteSize();
	std::string ss = boost::lexical_cast(size);
	boost::asio::async_write(socket, boost::asio::buffer(ss),
			boost::bind(&RpcChannel::SendSizeHandler, this, request,
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
