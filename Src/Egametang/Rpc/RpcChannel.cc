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
	RpcMetaPtr recv_meta(new RpcMeta());
	StringPtr recv_message(new std::string);
	RecvMeta(recv_meta, recv_message);
}

void RpcChannel::OnRecvMessage(RpcMetaPtr meta, StringPtr message)
{
	RpcHandlerPtr handler = handlers_[meta->id];
	handler->GetResponse()->ParseFromString(*message);

	handlers_.erase(meta->id);

	// read size
	RpcMetaPtr recv_meta(new RpcMeta());
	StringPtr recv_message(new std::string);
	RecvMeta(recv_meta, recv_message);

	// 回调放在函数最后.如果RecvMeta()放在回调之后,
	// 另外线程可能让io_service stop,导致RecvMeta还未跑完
	// 网络就终止了
	if (handler->GetDone() != NULL)
	{
		handler->GetDone()->Run();
	}
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
	handlers_[++id_] = handler;

	boost::hash<std::string> string_hash;

	StringPtr message(new std::string);
	request->SerializePartialToString(message.get());
	RpcMetaPtr meta(new RpcMeta());
	meta->size = message->size();
	meta->id = id_;
	meta->method = string_hash(method->full_name());
	SendMeta(meta, message);
}

} // namespace Egametang
