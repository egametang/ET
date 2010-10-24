#ifndef NET_ASYNSERVER_H
#define NET_ASYNSERVER_H

#include <string>
#include <boost/asio.hpp>
#include <boost/noncopyable.hpp>
#include "net/connection.h"

namespace hainan {

class asyn_server: private noncopyable
{
private:
	// hold all connection
	connection_set connections_;
	boost::asio::io_service io_service_;
	boost::asio::ip::tcp::acceptor acceptor_;
	connection_ptr new_connection_;

	void handle_accept(const system::error_code& e);
	void handle_stop();
public:
	explicit asyn_server(const string& address, const string& port);
	void start();
	void stop();
};
} // hainan

#endif // NET_ASYNSERVER_H
