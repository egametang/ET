#include <boost/asio.hpp>
#include <boost/bind.hpp>
#include <boost/make_shared.hpp>
#include <google/protobuf/service.h>
#include <google/protobuf/descriptor.h>
#include "Base/Marcos.h"
#include "Rpc/Typedef.h"
#include "Rpc/RpcServer.h"
#include "Rpc/RpcSession.h"
#include "Rpc/ResponseHandler.h"
#include "Rpc/MethodInfo.h"

namespace Egametang {

RpcServer::RpcServer(boost::asio::io_service& service, int port):
		ioService(service), acceptor(ioService),
		threadPool(1), sessions(),
		methods()
{
	boost::asio::ip::address address;
	address.from_string("127.0.0.1");
	boost::asio::ip::tcp::endpoint endpoint(address, port);
	acceptor.open(endpoint.protocol());
	acceptor.set_option(boost::asio::ip::tcp::acceptor::reuse_address(true));
	acceptor.bind(endpoint);
	acceptor.listen();
	auto newSession = boost::make_shared<RpcSession>(ioService, *this);
	acceptor.async_accept(newSession->Socket(),
			boost::bind(&RpcServer::OnAsyncAccept, this,
					newSession, boost::asio::placeholders::error));
}

RpcServer::~RpcServer()
{
	threadPool.wait();
	acceptor.close();
}

void RpcServer::OnAsyncAccept(RpcSessionPtr session, const boost::system::error_code& err)
{
	if (err)
	{
		return;
	}
	session->Start();
	sessions.insert(session);
	auto newSession = boost::make_shared<RpcSession>(ioService, *this);
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

void RpcServer::RunService(RpcSessionPtr session, RpcMetaPtr meta,
		StringPtr message, MessageHandler messageHandler)
{
	MethodInfoPtr methodInfo = methods[meta->method];

	auto responseHandler =
			boost::make_shared<ResponseHandler>(methodInfo, meta->id, messageHandler);
	responseHandler->Request()->ParseFromString(*message);

	google::protobuf::Closure* done = google::protobuf::NewCallback(
			this, &RpcServer::OnCallMethod,
			session, responseHandler);

	threadPool.schedule(
			boost::bind(&google::protobuf::Service::CallMethod, methodInfo->service,
					responseHandler->Method(), (google::protobuf::RpcController*)(nullptr),
					responseHandler->Request(), responseHandler->Response(),
					done));
}

void RpcServer::Register(ProtobufServicePtr service)
{
	boost::hash<std::string> stringHash;
	const google::protobuf::ServiceDescriptor* serviceDescriptor = service->GetDescriptor();
	for (int i = 0; i < serviceDescriptor->method_count(); ++i)
	{
		const google::protobuf::MethodDescriptor* methodDescriptor =
				serviceDescriptor->method(i);
		std::size_t methodHash = stringHash(methodDescriptor->full_name());
		auto methodInfo = boost::make_shared<MethodInfo>(service, methodDescriptor);
		methods[methodHash] = methodInfo;
	}
}

void RpcServer::Remove(RpcSessionPtr session)
{
	sessions.erase(session);
}

} // namespace Egametang
