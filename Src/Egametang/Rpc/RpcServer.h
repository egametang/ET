#ifndef RPC_RPC_SERVER_H
#define RPC_RPC_SERVER_H
#include <boost/asio.hpp>
#include <boost/function.hpp>
#include "Rpc/RpcTypedef.h"

namespace Egametang {

struct MethodInfo
{
	RpcServicePtr service;
	const google::protobuf::MethodDescriptor* method_descriptor;
	google::protobuf::Message* request_prototype;
	google::protobuf::Message* response_prototype;
	MethodInfo(RpcServicePtr service, const google::protobuf::MethodDescriptor* method_descriptor):
		service(service), method_descriptor(method_descriptor)
	{
		request_prototype = &service->GetRequestPrototype(method_descriptor);
		response_prototype = &service->GetResponsePrototype(method_descriptor);
	}
};

struct CallMethodBack
{
	google::protobuf::Message* request;
	google::protobuf::Message* response;
	RpcMetaPtr meta;
	SendResponseHandler send_response;

	CallMethodBack(
			google::protobuf::Message* request, google::protobuf::Message* response,
			RpcMetaPtr meta, SendResponseHandler& send_response):
				request(request), response(response),
				meta(meta), send_response(send_response)
	{
	}

	~CallMethodBack()
	{
		delete request;
		delete response;
	}

	void Run()
	{
		send_response(meta, response);
	}
};

class RpcServer: public boost::enable_shared_from_this<RpcServer>
{
private:
	typedef boost::unordered_set<RpcSessionPtr> RpcSessionSet;
	typedef boost::unordered_map<std::size_t, MethodInfoPtr> MethodMap;

	boost::asio::io_service& io_service_;
	boost::asio::ip::tcp::acceptor acceptor_;
	ThreadPool thread_pool_;
	RpcSessionSet sessions_;
	MethodMap methods_;

	void OnAsyncAccept(RpcSessionPtr session, const boost::system::error_code& err);
	void Callback(RpcSessionPtr session, CallMethodBackPtr call_method_back);

public:
	RpcServer(boost::asio::io_service& io_service, int port);
	~RpcServer();

	ThreadPool& ThreadPool();

	void RunService(RpcSessionPtr session, RpcMetaPtr meta,
			StringPtr message, SendResponseHandler handler);
	void RegisterService(RpcServicePtr service);
	void RemoveSession(RpcSessionPtr& session);
	void Stop();
};

} // namespace Egametang

#endif // RPC_RPC_SERVER_H
