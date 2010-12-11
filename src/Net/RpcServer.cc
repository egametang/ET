#include <boost/asio.hpp>
#include <boost/foreach.hpp>
#include "Base/Base.h"
#include "Net/RpcServer.h"
#include "Net/RpcSession.h"

namespace Hainan {

RpcServer::RpcServer(std::string& host, int port)
{
	boost::asio::ip::address address;
	address.from_string(host);
	boost::asio::ip::tcp::endpoint endpoint(address, port);
	acceptor.open(endpoint.protocol());
	acceptor.set_option(
			boost::asio::ip::tcp::acceptor::reuse_address(true));
	acceptor.bind(endpoint);
	acceptor.listen();
	RpcSessionPtr new_session(new RpcSession(io_service, sessions));
	acceptor.async_accept(new_session->socket(),
			boost::bind(&RpcServer::HandleAsyncAccept, this,
					boost::asio::placeholders::error));
}

void RpcServer::HandleAsyncAccept(
		RpcSessionPtr session, const boost::system::error_code& err)
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

void RpcServer::Start()
{
	io_service.run();
}

void RpcServer::Stop()
{
	acceptor.close();
	foreach(RpcSessionPtr session, rpc_server->sessions)
	{
		session->stop();
	}
	sessions.clear();
}

} // namespace Hainan
