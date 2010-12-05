//
// connection.hpp
// ~~~~~~~~~~~~~~
//
// Copyright (c) 2003-2010 Christopher M. Kohlhoff (chris at kohlhoff dot com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
//

#ifndef EXPERIMENTAL_HTTP_SERVER_CONNECTION_H
#define EXPERIMENTAL_HTTP_SERVER_CONNECTION_H

#include <boost/asio.hpp>
#include <boost/array.hpp>
#include <boost/noncopyable.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>
#include "Experimental/http_server/reply.h"
#include "Experimental/http_server/request.h"
#include "Experimental/http_server/request_handler.h"
#include "Experimental/http_server/request_parser.h"

namespace http_server {

class connection_manager;

/// Represents a single connection from a client.
class connection: public boost::enable_shared_from_this<connection>,
        private boost::noncopyable
{
public:
	/// Construct a connection with the given io_service.
	explicit connection(boost::asio::io_service& io_service,
	        connection_manager& manager, request_handler& handler);

	/// Get the socket associated with the connection.
	boost::asio::ip::tcp::socket& socket();

	/// Start the first asynchronous operation for the connection.
	void start();

	/// Stop all asynchronous operations associated with the connection.
	void stop();

private:
	/// Handle completion of a read operation.
	void handle_read(const boost::system::error_code& e,
	        std::size_t bytes_transferred);

	/// Handle completion of a write operation.
	void handle_write(const boost::system::error_code& e);

	/// Socket for the connection.
	boost::asio::ip::tcp::socket socket_;

	/// The manager for this connection.
	connection_manager& connection_manager_;

	/// The handler used to process the incoming request.
	request_handler& request_handler_;

	/// Buffer for incoming data.
	boost::array<char, 8192> buffer_;

	/// The incoming request.
	request request_;

	/// The parser for the incoming request.
	request_parser request_parser_;

	/// The reply to be sent back to the client.
	reply reply_;
};

typedef boost::shared_ptr<connection> connection_ptr;

} // namespace http_server

#endif // EXPERIMENTAL_HTTP_SERVER_CONNECTION_H
