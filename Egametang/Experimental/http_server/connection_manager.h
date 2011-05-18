//
// connection_manager.hpp
// ~~~~~~~~~~~~~~~~~~~~~~
//
// Copyright (c) 2003-2010 Christopher M. Kohlhoff (chris at kohlhoff dot com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
//

#ifndef EXPERIMENTAL_HTTP_SERVER_CONNECTION_MANAGER_H
#define EXPERIMENTAL_HTTP_SERVER_CONNECTION_MANAGER_H

#include <set>
#include <boost/noncopyable.hpp>
#include "Experimental/http_server/connection.h"

namespace http_server {

/// Manages open connections so that they may be cleanly stopped when the server
/// needs to shut down.
class connection_manager: private boost::noncopyable
{
public:
	/// Add the specified connection to the manager and start it.
	void start(connection_ptr c);

	/// Stop the specified connection.
	void stop(connection_ptr c);

	/// Stop all connections.
	void stop_all();

private:
	/// The managed connections.
	std::set<connection_ptr> connections_;
};

} // namespace http_server

#endif // EXPERIMENTAL_HTTP_SERVER_CONNECTION_MANAGER_H
