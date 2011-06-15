#include <boost/asio.hpp>
#include <boost/make_shared.hpp>
#include <google/protobuf/message.h>
#include "Rpc/RpcChannel.h"

namespace Egametang {

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

void RpcChannel::OnRecvMessage(
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

void RpcChannel::OnSendMessage(int32 id, RpcHandlerPtr handler,
		const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "SendMessage error:";
		return;
	}
	handlers_[id] = handler;
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

} // namespace Egametang
