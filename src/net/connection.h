#ifndef NET_CONNECTION_H
#define NET_CONNECTION_H

#include <string>
#include <boost/asio.hpp>
#include <boost/array.hpp>
#include <boost/noncopyable.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>

namespace hainan {

typedef boost::unordered_set<connection_ptr> connection_set;

class connection: private boost::noncopyable,
		public boost::enable_shared_from_this<connection>
{
private:

	boost::asio::ip::tcp::socket socket_;
	connection_set& connections_;
	boost::array<char, 8192> buffer_;
public:
	explicit connection::connection(boost::asio::io_service& io_service,
			connection_set& manager);
	boost::asio::ip::tcp::socket& socket();
	void start();
	void stop();
	virtual void handle_read(const boost::system::error_code& e,
			size_t bytes_transferred) = 0;
	virtual void handle_writer(const boost::system::error_code& e) = 0;
};
typedef boost::shared_ptr<connection> connection_ptr;

} // namespace hainan

#endif // NET_CONNECTION_H
