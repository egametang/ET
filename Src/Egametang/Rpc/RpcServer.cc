#include <boost/asio.hpp>
#include <boost/foreach.hpp>
#include <google/protobuf/service.h>
#include "Rpc/RpcTypedef.h"
#include "Rpc/RpcServer.h"
#include "Rpc/RpcSession.h"
#include "Thread/ThreadPool.h"
#include "Base/Marcos.h"

namespace Egametang {

RpcServer::RpcServer(boost::asio::io_service& io_service, int port, ThreadPool& thread_pool):
		io_service_(io_service), thread_pool_(thread_pool)
{
	boost::asio::ip::address address;
	address.from_string("localhost");
	boost::asio::ip::tcp::endpoint endpoint(address, port);
	acceptor_.open(endpoint.protocol());
	acceptor_.set_option(boost::asio::ip::tcp::acceptor::reuse_address(true));
	acceptor_.bind(endpoint);
	acceptor_.listen();
	RpcSessionPtr new_session(new RpcSession(sessions_));
	acceptor_.async_accept(new_session->socket(),
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
	acceptor_.async_accept(new_session->socket(),
			boost::bind(&RpcServer::OnAsyncAccept, this,
					boost::asio::placeholders::error));
}

void RpcServer::Callback(RpcSessionPtr session,
		boost::function<void (RpcSessionPtr, RpcResponsePtr)> handler)
{
	session->socket.get_io_service().post(handler);
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

void RpcServer::RunService(RpcSessionPtr session, RpcRequestPtr request,
		boost::function<void (RpcSessionPtr, RpcResponsePtr)> handler)
{
	google::protobuf::Closure* done = google::protobuf::NewCallback(
			this, &RpcServer::Callback, session, handler);
	thread_pool_.PushTask(
			boost::bind(&google::protobuf::Service::CallMethod, &service_,
					method, NULL, request.get(), done));
}

void RpcServer::RegisterService(ProtobufServicePtr service)
{

}

void RpcServer::Start()
{
	io_service_.run();
}

void RpcServer::RemoveSession(RpcSessionPtr& session)
{
	sessions_.erase(session);
}

} // namespace Egametang
