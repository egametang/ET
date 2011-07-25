#include <boost/asio.hpp>
#include <boost/foreach.hpp>
#include <boost/bind.hpp>
#include <google/protobuf/service.h>
#include <google/protobuf/descriptor.h>
#include <glog/logging.h>
#include "Base/Marcos.h"
#include "Rpc/RpcTypedef.h"
#include "Rpc/RpcServer.h"
#include "Rpc/RpcSession.h"
#include "Rpc/ResponseHandler.h"
#include "Rpc/MethodInfo.h"

namespace Egametang {

RpcServer::RpcServer(boost::asio::io_service& service, int port):
		io_service(service), acceptor(io_service), thread_pool()
{
	boost::asio::ip::address address;
	address.from_string("127.0.0.1");
	boost::asio::ip::tcp::endpoint endpoint(address, port);
	acceptor.open(endpoint.protocol());
	acceptor.set_option(boost::asio::ip::tcp::acceptor::reuse_address(true));
	acceptor.bind(endpoint);
	acceptor.listen();
	RpcSessionPtr new_session(new RpcSession(*this));
	acceptor.async_accept(new_session->Socket(),
			boost::bind(&RpcServer::OnAsyncAccept, this,
					new_session, boost::asio::placeholders::error));
}

boost::asio::io_service& RpcServer::IOService()
{
	return io_service;
}

void RpcServer::OnAsyncAccept(RpcSessionPtr session, const boost::system::error_code& err)
{
	if (err)
	{
		LOG(ERROR) << "accept fail: " << err.message();
		return;
	}
	session->Start();
	sessions.insert(session);
	RpcSessionPtr new_session(new RpcSession(*this));
	acceptor.async_accept(new_session->Socket(),
			boost::bind(&RpcServer::OnAsyncAccept, this,
					new_session, boost::asio::placeholders::error));
}

void RpcServer::OnCallMethod(RpcSessionPtr session, ResponseHandlerPtr response_handler)
{
	// push到网络线程
	session->Socket().get_io_service().post(
			boost::bind(&ResponseHandler::Run, response_handler));
}

void RpcServer::Stop()
{
	thread_pool.Wait();
	acceptor.close();
	foreach(RpcSessionPtr session, sessions)
	{
		session->Stop();
	}
	sessions.clear();
}

void RpcServer::RunService(RpcSessionPtr session, RpcMetaPtr meta,
		StringPtr message, MessageHandler message_handler)
{
	MethodInfoPtr method_info = methods[meta->method];

	ResponseHandlerPtr response_handler(
			new ResponseHandler(method_info, meta->id, message_handler));
	response_handler->Request()->ParseFromString(*message);

	google::protobuf::Closure* done = google::protobuf::NewCallback(
			this, &RpcServer::OnCallMethod,
			session, response_handler);

	thread_pool.Schedule(
			boost::bind(&google::protobuf::Service::CallMethod, method_info->service,
					response_handler->Method(), (google::protobuf::RpcController*)(NULL),
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
		CHECK(methods.find(method_hash) == methods.end());
		methods[method_hash] = method_info;
	}
}

void RpcServer::RemoveSession(RpcSessionPtr& session)
{
	sessions.erase(session);
}

} // namespace Egametang
