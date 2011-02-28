#include <boost/asio.hpp>
#include <boost/make_shared.hpp>
#include <google/protobuf/message.h>
#include "Net/RpcChannel.h"

namespace Hainan {

RpcChannel::RpcChannel(
		boost::asio::io_service& io_service, std::string& host, int port):
		io_service_(io_service)
{
	// another thread?
	boost::asio::ip::address address;
	address.from_string(host);
	boost::asio::ip::tcp::endpoint endpoint(address, port);
	socket_.async_connect(endpoint,
			boost::bind(&RpcChannel::AsyncConnectHandler, this,
					boost::asio::placeholders::error));
}

void RpcChannel::AsyncConnectHandler(const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "async connect failed";
		return;
	}
	RecvMessegeSize();
}

void RpcChannel::RecvMessegeSize()
{
	IntPtr size(new int);
	boost::asio::async_read(socket_,
			boost::asio::buffer(
					reinterpret_cast<char*>(size.get()), sizeof(int)),
			boost::bind(&RpcChannel::RecvMessage, this, size,
					boost::asio::placeholders::error));
}

void RpcChannel::RecvMessage(IntPtr size, const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "receive response size failed";
		return;
	}
	StringPtr ss;
	boost::asio::async_read(socket_,
			boost::asio::buffer(*ss, *size),
			boost::bind(&RpcChannel::RecvMessageHandler, this, ss,
					boost::asio::placeholders::error));
}

void RpcChannel::RecvMessageHandler(
		StringPtr ss, const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "receive response failed";
		return;
	}

	RpcResponse response;
	Response->ParseFromString(*ss);
	RpcHandlerPtr handler = handlers_[response.id()];
	handler->GetResponse()->ParseFromString(response.response());

	if (handler->done_ != NULL)
	{
		handler->done_->Run();
	}

	handlers_.erase(response.id());

	// read size
	RecvMessegeSize();
}

void RpcChannel::SendMessageHandler(int32 id, RpcHandlerPtr handler,
		const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "SendMessage error:";
		return;
	}
	handlers_[id] = handler;
}

void RpcChannel::SendMessage(const RpcRequestPtr request,
		RpcHandlerPtr handler, const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "SendRequestSize error:";
		return;
	}
	std::string ss = request->SerializeAsString();
	boost::asio::async_write(socket_, boost::asio::buffer(ss),
			boost::bind(&RpcChannel::SendMessageHandler, this, request->id(),
					handler, boost::asio::placeholders::error));
}

void RpcChannel::SendMessageSize(
		const RpcRequestPtr request, RpcHandlerPtr handler)
{
	int size = request->ByteSize();
	std::string ss = boost::lexical_cast(size);
	boost::asio::async_write(socket_, boost::asio::buffer(ss),
			boost::bind(&RpcChannel::SendMessage, this, request,
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
	req->set_id(++id_);
	req->set_method(method->full_name());
	req->set_request(request->SerializeAsString());
	RpcHandlerPtr handler(new RpcHandler(controller, response, done));
	SendMessageSize(req, handler);
}

} // namespace Hainan
