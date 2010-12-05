//
// request.hpp
// ~~~~~~~~~~~
//
// Copyright (c) 2003-2010 Christopher M. Kohlhoff (chris at kohlhoff dot com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
//

#ifndef EXPERIMENTAL_HTTP_SERVER_HTTP_REQUEST_H
#define EXPERIMENTAL_HTTP_SERVER_HTTP_REQUEST_H

#include <string>
#include <vector>
#include "Experimental/http_server/header.h"

namespace http_server {

/// A request received from a client.
struct request
{
	std::string method;
	std::string uri;
	int http_version_major;
	int http_version_minor;
	std::vector<header> headers;
};

} // namespace http_server

#endif // EXPERIMENTAL_HTTP_SERVER_HTTP_REQUEST_H
