//
// request_handler.hpp
// ~~~~~~~~~~~~~~~~~~~
//
// Copyright (c) 2003-2010 Christopher M. Kohlhoff (chris at kohlhoff dot com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
//

#ifndef EXPERIMENTAL_HTTP_SERVER_HTTP_REQUEST_HANDLER_H
#define EXPERIMENTAL_HTTP_SERVER_HTTP_REQUEST_HANDLER_H

#include <string>
#include <boost/noncopyable.hpp>

namespace http_server {

struct reply;
struct request;

/// The common handler for all incoming requests.
class request_handler: private boost::noncopyable
{
public:
	/// Construct with a directory containing files to be served.
	explicit request_handler(const std::string& doc_root);

	/// Handle a request and produce a reply.
	void handle_request(const request& req, reply& rep);

private:
	/// The directory containing the files to be served.
	std::string doc_root_;

	/// Perform URL-decoding on a string. Returns false if the encoding was
	/// invalid.
	static bool url_decode(const std::string& in, std::string& out);
};

} // namespace http_server

#endif // EXPERIMENTAL_HTTP_SERVER_HTTP_REQUEST_HANDLER_H
