#include <boost/foreach.hpp>
#include "base/base.h"
#include "net/asyn_server.h"
#include "net/connection.h"

namespace hainan {

explicit asyn_server::asyn_server(const string& address, const string& port) :
	io_service_(), acceptor_(io_service_),
	new_connection_(new connection(io_service_, connections_))
{
	boost::asio::ip::tcp::resolver resolver(io_service_);
	boost::asio::ip::tcp::resolver::query query(address, port);
	boost::asio::ip::tcp::endpoint endpoint = *resolver.resolve(query);
	acceptor_.open(endpoint.protocol());
	acceptor_.set_option(boost::asio::ip::tcp::acceptor::reuse_address(true));
	acceptor_.bind(endpoint);
	acceptor_.listen();
	acceptor_.async_accept(new_connection_->socket(),
			boost::bind(&server::handle_accept, this,
					asio::placeholders::error));
}

void asyn_server::handle_accept(const system::error_code& e)
{
	if (!e)
	{
		connections_.insert(new_connection_);
		new_connection_.reset(new connection(io_service_, connections_));
		acceptor_.async_accept(new_connection_->socket(),
				boost::bind(&server::handle_accept, this,
						asio::placeholders::error));
	}
}

void asyn_server::handle_stop()
{
	acceptor_.close();
	foreach(connection_ptr connection, connections_)
	{
		connection->stop();
	}
	connections_.clear();
}

void asyn_server::start()
{
	io_service_.run();
}

void asyn_server::stop()
{
	io_service_.post(boost::bind(&asyn_server::handle_stop(), this));
}

} // namespace hainan
