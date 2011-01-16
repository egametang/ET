#include <boost/asio.hpp>
#include <boost/foreach.hpp>
#include <google/protobuf/service.h>
#include "Base/Base.h"
#include "Net/RpcServer.h"
#include "Net/RpcSession.h"
#include "Thread/ThreadPool.h"

namespace Hainan {

RpcServer::RpcServer(boost::asio::io_service& io_service, int port):
		io_service(io_service), thread_pool()
{
	boost::asio::ip::address address;
	address.from_string("localhost");
	boost::asio::ip::tcp::endpoint endpoint(address, port);
	acceptor.open(endpoint.protocol());
	acceptor.set_option(boost::asio::ip::tcp::acceptor::reuse_address(true));
	acceptor.bind(endpoint);
	acceptor.listen();
	RpcSessionPtr new_session(new RpcSession(sessions));
	acceptor.async_accept(new_session->socket(),
			boost::bind(&RpcServer::HandleAsyncAccept, this,
					boost::asio::placeholders::error));
}

void RpcServer::HandleAsyncAccept(RpcSessionPtr session, const boost::system::error_code& err)
{
	if (err)
	{
		return;
	}
	session->Start();
	sessions.insert(session);
	RpcSessionPtr new_session(new RpcSession(io_service, sessions));
	acceptor.async_accept(new_session->socket(),
			boost::bind(&RpcServer::HandleAsyncAccept, this,
					boost::asio::placeholders::error));
}

void RpcServer::Callback(RpcSessionPtr session,
		boost::function<void (RpcSessionPtr, RpcResponsePtr)> handler)
{
	session->socket.get_io_service().post(handler);
}

void RpcServer::Stop()
{
	acceptor.close();
	foreach(RpcSessionPtr session, rpc_server->sessions)
	{
		session->Stop();
	}
	sessions.clear();
}

void RpcServer::RunService(RpcSessionPtr session, RpcRequestPtr request,
		boost::function<void (RpcSessionPtr, RpcResponsePtr)> handler)
{
	google::protobuf::Closure* done = google::protobuf::NewCallback(
			this, &RpcServer::Callback, session, handler);
	thread_pool.PushTask(
			boost::bind(&google::protobuf::Service::CallMethod, &service,
					method, NULL, request.get(), done));
}

void RpcServer::RegisterService(ProtobufServicePtr service)
{

}

void RpcServer::Start()
{
	io_service.run();
}

} // namespace Hainan
