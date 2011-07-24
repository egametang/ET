#include <boost/asio.hpp>
#include <boost/foreach.hpp>
#include <google/protobuf/service.h>
#include <glog/logging.h>
#include "Base/Marcos.h"
#include "Rpc/RpcTypedef.h"
#include "Rpc/RpcServer.h"
#include "Rpc/RpcSession.h"
#include "Rpc/ResponseHandler.h"
#include "Rpc/MethodInfo.h"
#include "Thread/ThreadPool.h"

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

boost::asio::io_service& RpcServer::IOService()
{
	return io_service_;
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

void RpcServer::OnCallMethod(RpcSessionPtr session, ResponseHandlerPtr response_handler)
{
	// push到网络线程
	session->Socket().get_io_service().post(
			boost::bind(&ResponseHandler::Run, response_handler));
}

void RpcServer::Stop()
{
	thread_pool_.Wait();
	acceptor_.close();
	foreach(RpcSessionPtr session, sessions_)
	{
		session->Stop();
	}
	sessions_.clear();
}

void RpcServer::RunService(RpcSessionPtr session, RpcMetaPtr meta,
		StringPtr message, MessageHandler message_handler)
{
	MethodInfoPtr method_info = methods_[meta->method];

	ResponseHandlerPtr response_handler(
			new ResponseHandler(method_info, meta->id, message_handler));
	response_handler->Request()->ParseFromString(*message);

	google::protobuf::Closure* done = google::protobuf::NewCallback(
			shared_from_this(), &RpcServer::OnCallMethod,
			session, response_handler);

	thread_pool_.Schedule(
			boost::bind(&google::protobuf::Service::CallMethod, this,
					response_handler->Method(), NULL,
					response_handler->Request(), response_handler->Response(),
					done));
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
