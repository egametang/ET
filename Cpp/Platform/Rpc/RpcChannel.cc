#include <boost/asio.hpp>
#include <boost/bind.hpp>
#include <glog/logging.h>
#include <google/protobuf/message.h>
#include <google/protobuf/descriptor.h>
#include "Rpc/RpcCommunicator.h"
#include "Rpc/RpcChannel.h"
#include "Rpc/RequestHandler.h"

namespace Egametang {

RpcChannel::RpcChannel(boost::asio::io_service& ioService, std::string host, int port):
		RpcCommunicator(ioService), id(0)
{
	// another thread?
	boost::asio::ip::address address;
	address.from_string(host);
	boost::asio::ip::tcp::endpoint endpoint(address, port);
	socket.async_connect(endpoint,
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
	RpcMetaPtr recvMeta(new RpcMeta());
	StringPtr recvMessage(new std::string);
	RecvMeta(recvMeta, recvMessage);
}

void RpcChannel::OnRecvMessage(RpcMetaPtr meta, StringPtr message)
{
	RequestHandlerPtr requestHandler = requestHandlers[meta->id];
	requestHandlers.erase(meta->id);

	requestHandler->Response()->ParseFromString(*message);

	// meta和message可以循环利用
	RecvMeta(meta, message);
	// 回调放在函数最后.如果RecvMeta()放在回调之后,
	// 另外线程可能让io_service stop,导致RecvMeta还未跑完
	// 网络就终止了
	requestHandler->Run();
}

void RpcChannel::OnSendMessage(RpcMetaPtr meta, StringPtr message)
{
}

void RpcChannel::Stop()
{
	if (isStopped)
	{
		return;
	}
	isStopped = true;
	socket.close();
}

void RpcChannel::CallMethod(
		const google::protobuf::MethodDescriptor* method,
		google::protobuf::RpcController* controller,
		const google::protobuf::Message* request,
		google::protobuf::Message* response,
		google::protobuf::Closure* done)
{
	RequestHandlerPtr request_handler(new RequestHandler(response, done));
	requestHandlers[++id] = request_handler;

	boost::hash<std::string> stringHash;

	StringPtr message(new std::string);
	request->SerializePartialToString(message.get());
	RpcMetaPtr meta(new RpcMeta());
	meta->size = message->size();
	meta->id = id;
	meta->method = stringHash(method->full_name());
	SendMeta(meta, message);
}

} // namespace Egametang
