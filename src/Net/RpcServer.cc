#include <boost/asio.hpp>
#include "Net/RpcServer.h"
#include "Net/RpcSession.h"

namespace Hainan {

typedef boost::unordered_set<RpcSessionPtr> RpcSessionSet;

struct RpcServer::RpcServerFeild
{
	boost::asio::io_service io_service;
	boost::asio::ip::tcp::acceptor acceptor;
	RpcSessionSet sessions;

	RpcServerFeild();
	~RpcServerFeild();
};

RpcServer::RpcServerFeild::RpcServerFeild():
		io_service(), acceptor(io_service)
{
}

RpcServer::RpcServerFeild::~RpcServerFeild()
{
}

RpcServer::RpcServer(std::string& host, int port):
		rpc_server(new RpcServer::RpcServerFeild())
{
	boost::asio::ip::address address;
	address.from_string(host);
	boost::asio::ip::tcp::endpoint endpoint(address, port);
	rpc_server->acceptor.open(endpoint.protocol());
	rpc_server->acceptor.set_option(
			boost::asio::ip::tcp::acceptor::reuse_address(true));
	rpc_server->acceptor.bind(endpoint);
	rpc_server->acceptor.listen();
	RpcSessionPtr session(new RpcSession(
			rpc_server->io_service,
			rpc_server->sessions));
	rpc_server->acceptor.async_accept(session->socket(),
			boost::bind(&RpcServer::HandleAsyncAccept, this,
					boost::asio::placeholders::error));
}

void RpcServer::HandleAsyncAccept(RpcSessionSet session,
		const boost::system::error_code& err)
{
	if (err)
	{
		return;
	}
	session->Start();
	rpc_server->sessions.insert(session);
	RpcSessionPtr session(new RpcSession(
			rpc_server->io_service,
			rpc_server->sessions));
	rpc_server->acceptor.async_accept(session->socket(),
			boost::bind(&RpcServer::HandleAsyncAccept, this,
					boost::asio::placeholders::error));
}

void RpcServer::AsyncConnect()
{
}

void RpcServer::Stop()
{
}

} // namespace Hainan
