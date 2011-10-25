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
		ioService(service), acceptor(ioService),
		threadPool(), sessions(),
		methods()
{
	boost::asio::ip::address address;
	address.from_string("127.0.0.1");
	boost::asio::ip::tcp::endpoint endpoint(address, port);
	acceptor.open(endpoint.protocol());
	acceptor.set_option(boost::asio::ip::tcp::acceptor::reuse_address(true));
	acceptor.bind(endpoint);
	acceptor.listen();
	RpcSessionPtr new_session(new RpcSession(ioService, *this));
	acceptor.async_accept(new_session->Socket(),
			boost::bind(&RpcServer::OnAsyncAccept, this,
					new_session, boost::asio::placeholders::error));
}

RpcServer::~RpcServer()
{
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
	RpcSessionPtr newSession(new RpcSession(ioService, *this));
	acceptor.async_accept(newSession->Socket(),
			boost::bind(&RpcServer::OnAsyncAccept, this,
					newSession, boost::asio::placeholders::error));
}

void RpcServer::OnCallMethod(RpcSessionPtr session, ResponseHandlerPtr responseHandler)
{
	// 调度到网络线
	session->Socket().get_io_service().post(
			boost::bind(&ResponseHandler::Run, responseHandler));
}

void RpcServer::Stop()
{
	threadPool.Wait();
	acceptor.close();
	sessions.clear();
}

void RpcServer::RunService(RpcSessionPtr session, RpcMetaPtr meta,
		StringPtr message, MessageHandler messageHandler)
{
	MethodInfoPtr methodInfo = methods[meta->method];

	ResponseHandlerPtr responseHandler(
			new ResponseHandler(methodInfo, meta->id, messageHandler));
	responseHandler->Request()->ParseFromString(*message);

	google::protobuf::Closure* done = google::protobuf::NewCallback(
			this, &RpcServer::OnCallMethod,
			session, responseHandler);

	threadPool.Schedule(
			boost::bind(&google::protobuf::Service::CallMethod, methodInfo->service,
					responseHandler->Method(), (google::protobuf::RpcController*)(NULL),
					responseHandler->Request(), responseHandler->Response(),
					done));
}

void RpcServer::Register(RpcServicePtr service)
{
	boost::hash<std::string> stringHash;
	const google::protobuf::ServiceDescriptor* serviceDescriptor = service->GetDescriptor();
	for (int i = 0; i < serviceDescriptor->method_count(); ++i)
	{
		const google::protobuf::MethodDescriptor* methodDescriptor =
				serviceDescriptor->method(i);
		std::size_t methodHash = stringHash(methodDescriptor->full_name());
		MethodInfoPtr methodInfo(new MethodInfo(service, methodDescriptor));
		CHECK(methods.find(methodHash) == methods.end());
		methods[methodHash] = methodInfo;
	}
}

void RpcServer::Remove(RpcSessionPtr& session)
{
	sessions.erase(session);
}

} // namespace Egametang
