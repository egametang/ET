#include <vector>
#include <boost/bind.hpp>
#include "net/connection.h"

namespace hainan {

connection::connection(boost::asio::io_service& io_service,
		connection_set& manager):
	socket_(io_service), connections_(manager)
{
}

boost::asio::ip::tcp::socket& connection::socket()
{
	return socket_;
}

void connection::start()
{
	socket_.async_read_some(boost::asio::buffer(buffer_),
			boost::bind(&connection::handle_read, shared_from_this(),
					boost::asio::placeholders::error,
					boost::asio::placeholders::bytes_transferred));
}

void connection::stop()
{
	socket_.close();
}

} // namespace hainan
