#ifndef NET_ASYNSERVER_H
#define NET_ASYNSERVER_H

#include <string>
#include <boost/asio.hpp>
#include <boost/noncopyable.hpp>

namespace hainan {

using namespace std;
using namespace boost;

class asyn_server: private noncopyable
{
private:
	typedef unordered_set<connection> connection_set;

	// hold all connection
	connectionSet all_connections;
	asio::io_service io_service;
	asio::ip::tcp::acceptor acceptor;
	connection_ptr new_connection;

	void handle_accept(const system::error_code& e);
	void handle_stop();
public:
	explicit asyn_server(const string& address, const string& port);
	void start();
	void stop();
};
} // hainan

#endif // NET_ASYNSERVER_H
