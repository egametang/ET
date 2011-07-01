#include <boost/asio.hpp>
#include <boost/bind.hpp>
#include <glog/logging.h>
#include <google/protobuf/message.h>
#include <google/protobuf/descriptor.h>
#include "Rpc/RpcCommunicator.h"
#include "Rpc/RpcChannel.h"
#include "Rpc/RpcHandler.h"

namespace Egametang {

RpcChannel::RpcChannel(boost::asio::io_service& io_service, std::string host, int port):
		RpcCommunicator(io_service), id_(0)
{
	// another thread?
	boost::asio::ip::address address;
	address.from_string(host);
	boost::asio::ip::tcp::endpoint endpoint(address, port);
	socket_.async_connect(endpoint,
			boost::bind(&RpcChannel::OnAsyncConnect, this,
					boost::asio::placeholders::error));
}

RpcChannel::~RpcChannel()
{
}

void RpcChannel::OnAsyncConnect(const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "async connect failed: " << err.message();
		return;
	}
	RecvMeta();
}

void RpcChannel::OnRecvMessage(StringPtr ss)
{
	RpcResponse response;
	response.ParseFromString(*ss);
	RpcHandlerPtr handler = handlers_[response.id()];
	handler->GetResponse()->ParseFromString(response.response());

	handlers_.erase(response.id());

	if (handler->GetDone() != NULL)
	{
		handler->GetDone()->Run();
	}

	// read size
	RecvMeta();
}

void RpcChannel::OnSendMessage()
{
}

void RpcChannel::Stop()
{
	RpcCommunicator::Stop();
}

void RpcChannel::CallMethod(
		const google::protobuf::MethodDescriptor* method,
		google::protobuf::RpcController* controller,
		const google::protobuf::Message* request,
		google::protobuf::Message* response,
		google::protobuf::Closure* done)
{
	RpcHandlerPtr handler(new RpcHandler(controller, response, done));
	handlers_[id_] = handler;

	boost::hash<std::string> string_hash;

	std::string message = request->SerializeAsString();
	RpcMeta meta;
	meta.size = message.size();
	meta.id = ++id_;
	meta.opcode = string_hash(method->full_name());
	meta.checksum = string_hash(message);

	SendMeta(meta, message);
}

} // namespace Egametang
