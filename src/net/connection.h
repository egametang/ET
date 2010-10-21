#ifndef NET_CONNECTION_H
#define NET_CONNECTION_H

#include <string>
#include <boost/asio.hpp>
#include <boost/array.hpp>
#include <boost/noncopyable.hpp>
#include <boost/shared_ptr.hpp>

namespace hainan {

using namespace std;
using namespace boost;

class connection: private noncopyable
{
private:
	typedef unordered_set<connection> connection_set;

	asio::ip::tcp::socket socket;
	connection_set& all_connections;
	boost::array<char, 8192> buffer;
public:
	explicit connection(asio::io_service& io_service);
	void start();
	void stop();
	virtual void handle_read();
	virtual void handle_writer();
};
typedef shared_ptr<connection> connection_ptr;

} // namespace hainan

#endif // NET_CONNECTION_H
