#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include <boost/make_shared.hpp>
#include <google/protobuf/message.h>
#include <google/protobuf/descriptor.h>
#include "Rpc/RpcCommunicator.h"
#include "Rpc/RpcClient.h"
#include "Rpc/RequestHandler.h"

namespace Egametang {

RpcClient::RpcClient(boost::asio::io_service& ioService, std::string host, int port):
		RpcCommunicator(ioService), id(0)
{
	// another thread?
	boost::asio::ip::address address;
	address.from_string(host);
	boost::asio::ip::tcp::endpoint endpoint(address, port);
	socket.async_connect(endpoint,
			boost::bind(&RpcClient::OnAsyncConnect, this,
					boost::asio::placeholders::error));
}

RpcClient::~RpcClient()
{
}

void RpcClient::OnAsyncConnect(const boost::system::error_code& err)
{
	if (err)
	{
		return;
	}
	auto recvMeta = boost::make_shared<RpcMeta>();
	auto recvMessage = boost::make_shared<std::string>();
	RecvMeta(recvMeta, recvMessage);
}

void RpcClient::OnRecvMessage(RpcMetaPtr meta, StringPtr message)
{
	// 没有回调
	if (requestHandlers.find(meta->id) == requestHandlers.end())
	{
		// meta和message可以循环利用
		RecvMeta(meta, message);
	}
	else
	{
		auto requestHandler = requestHandlers[meta->id];
		requestHandlers.erase(meta->id);
		requestHandler->Response()->ParseFromString(*message);
		// meta和message可以循环利用
		RecvMeta(meta, message);
		// 回调放在函数最.如果RecvMeta()放在回调之后,
		// 另外线程可能让io_service stop,导致RecvMeta还未跑完
		// 网络就终止了
		requestHandler->Run();
	}
}

void RpcClient::OnSendMessage(RpcMetaPtr meta, StringPtr message)
{
}

void RpcClient::CallMethod(
		const google::protobuf::MethodDescriptor* method,
		google::protobuf::RpcController* controller,
		const google::protobuf::Message* request,
		google::protobuf::Message* response,
		google::protobuf::Closure* done)
{
	if (done)
	{
		auto request_handler = boost::make_shared<RequestHandler>(response, done);
		requestHandlers[++id] = request_handler;
	}
	std::hash<std::string> stringHash;
	auto message = boost::make_shared<std::string>();
	request->SerializePartialToString(message.get());
	auto meta = boost::make_shared<RpcMeta>();
	meta->size = message->size();
	meta->id = id;
	meta->method = stringHash(method->full_name());
	SendMeta(meta, message);
}

} // namespace Egametang
