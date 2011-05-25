//
// server.hpp
// ~~~~~~~~~~
//
// Copyright (c) 2003-2010 Christopher M. Kohlhoff (chris at kohlhoff dot com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
//

#ifndef EXPERIMENTAL_HTTP_SERVER_HTTP_SERVER_H
#define EXPERIMENTAL_HTTP_SERVER_HTTP_SERVER_H

#include <boost/asio.hpp>
#include <string>
#include <boost/noncopyable.hpp>
#include "Experimental/http_server/connection.h"
#include "Experimental/http_server/connection_manager.h"
#include "Experimental/http_server/request_handler.h"

namespace http_server {

/// The top-level class of the HTTP server.
class server: private boost::noncopyable
{
public:
	/// Construct the server to listen on the specified TCP address and port, and
	/// serve up files from the given directory.
	explicit server(const std::string& address, const std::string& port,
	        const std::string& doc_root);

	/// Run the server's io_service loop.
	void run();

	/// Stop the server.
	void stop();

private:
	/// Handle completion of an asynchronous accept operation.
	void handle_accept(const boost::system::error_code& e);

	/// Handle a request to stop the server.
	void handle_stop();

	/// The io_service used to perform asynchronous operations.
	boost::asio::io_service io_service_;

	/// Acceptor used to listen for incoming connections.
	boost::asio::ip::tcp::acceptor acceptor_;

	/// The connection manager which owns all live connections.
	connection_manager connection_manager_;

	/// The next connection to be accepted.
	connection_ptr new_connection_;

	/// The handler for all incoming requests.
	request_handler request_handler_;
};

} // namespace http_server

#endif // EXPERIMENTAL_HTTP_SERVER_HTTP_SERVER_H
