//
// header.hpp
// ~~~~~~~~~~
//
// Copyright (c) 2003-2010 Christopher M. Kohlhoff (chris at kohlhoff dot com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
//

#ifndef EXPERIMENTAL_HTTP_SERVER_HEADER_H
#define EXPERIMENTAL_HTTP_SERVER_HEADER_H

#include <string>

namespace http_server {

struct header
{
	std::string name;
	std::string value;
};

} // namespace http_server

#endif // EXPERIMENTAL_HTTP_SERVER_HEADER_H
