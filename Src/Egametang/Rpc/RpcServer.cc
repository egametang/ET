#include <boost/asio.hpp>
#include <boost/foreach.hpp>
#include <google/protobuf/service.h>
#include <glog/logging.h>
#include "Rpc/RpcTypedef.h"
#include "Rpc/RpcServer.h"
#include "Rpc/RpcSession.h"
#include "Thread/ThreadPool.h"
#include "Base/Marcos.h"

namespace Egametang {

RpcServer::RpcServer(boost::asio::io_service& io_service, int port):
		io_service_(io_service), thread_pool_()
{
	boost::asio::ip::address address;
	address.from_string("localhost");
	boost::asio::ip::tcp::endpoint endpoint(address, port);
	acceptor_.open(endpoint.protocol());
	acceptor_.set_option(boost::asio::ip::tcp::acceptor::reuse_address(true));
	acceptor_.bind(endpoint);
	acceptor_.listen();
	RpcSessionPtr new_session(new RpcSession(*this));
	acceptor_.async_accept(new_session->Socket(),
			boost::bind(&RpcServer::OnAsyncAccept, this,
					new_session, boost::asio::placeholders::error));
}

void RpcServer::OnAsyncAccept(RpcSessionPtr session, const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "accept fail: " << err.message();
		return;
	}
	session->Start();
	sessions_.insert(session);
	RpcSessionPtr new_session(new RpcSession(*this));
	acceptor_.async_accept(new_session->Socket(),
			boost::bind(&RpcServer::OnAsyncAccept, this,
					boost::asio::placeholders::error));
}

ThreadPool& RpcServer::ThreadPool()
{
	return thread_pool_;
}

void RpcServer::Callback(RpcSessionPtr session, CallMethodBackPtr call_method_back)
{
	RpcMetaPtr meta = call_method_back->meta;
	StringPtr message(new std::string);
	google::protobuf::Message* request = call_method_back->request;
	google::protobuf::Message* response = call_method_back->response;
	response->SerializeToString(message.get());
	meta->size = message->size();

	session->Socket().get_io_service().post(
			boost::bind(&CallMethodBack::Run, call_method_back,
					meta, response));
}

void RpcServer::Stop()
{
	acceptor_.close();
	foreach(RpcSessionPtr session, sessions_)
	{
		session->Stop();
	}
	sessions_.clear();
}

void RpcServer::RunService(RpcSessionPtr session, RpcMetaPtr meta,
		StringPtr message, SendResponseHandler handler)
{
	MethodInfoPtr method_info = methods_[meta->method];
	const google::protobuf::MethodDescriptor* method = method_info->method_descriptor;
	// 这两个Message在CallMethodBack里面delete
	google::protobuf::Message* request = method_info->request_prototype->New();
	google::protobuf::Message* response = method_info->response_prototype->New();
	request->ParseFromString(*message);

	CallMethodBackPtr call_method_back(new CallMethodBack(request, response, meta, handler));
	google::protobuf::Closure* done = google::protobuf::NewCallback(
			shared_from_this(), &RpcServer::Callback,
			session, call_method_back);

	thread_pool_.Schedule(
			boost::bind(&google::protobuf::Service::CallMethod, shared_from_this(),
					method, NULL, request, response, done));
}

void RpcServer::RegisterService(RpcServicePtr service)
{
	boost::hash<std::string> string_hash;
	const google::protobuf::ServiceDescriptor* service_descriptor = service->GetDescriptor();
	for (int i = 0; i < service_descriptor->method_count(); ++i)
	{
		const google::protobuf::MethodDescriptor* method_descriptor =
				service_descriptor->method(i);
		std::size_t method_hash = string_hash(method_descriptor->full_name());
		MethodInfoPtr method_info(new MethodInfo(service, method_descriptor));
		CHECK(methods_.find(method_hash) == methods_.end());
		methods_[method_hash] = method_info;
	}
}

void RpcServer::RemoveSession(RpcSessionPtr& session)
{
	sessions_.erase(session);
}

} // namespace Egametang
