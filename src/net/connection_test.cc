#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include "net/connection.h"

namespace hainan {

static const char address[] = "127.0.0.1";
static int port = 10000;
class connection1: public connection
{
public:
	std::string content;
	size_t bytes;
	explicit connection1(boost::asio::io_service io_service,
			connection_set connections):
		connection(io_service, connections)
	{
	}

	void handle_read(const system::error_code& e,
			size_t bytes_transferred)
	{
		if (!e)
		{
			content = buffer_.c_array();
			bytes = bytes_transferred;
		}
	}

	void handle_write()
	{

	}
};

class ConnectionTest: public testing::Test
{
private:
	boost::asio::io_service io_service_;
	boost::asio::ip::tcp::acceptor acceptor_;

	connection_set connections_;
	connection_ptr connection_;

	void SetUp()
	{
		connection_.reset(new connection1(io_service_, connections_));
	}

	void handle_accept()
	{
	}

	void server_start()
	{
		boost::asio::ip::tcp::endpoint endpoint(
				boost::asio::ip::address_v4.from_string(address), port);
		acceptor_.open(endpoint.protocol());
		acceptor_.set_option(boost::asio::ip::tcp::acceptor::reuse_address(true));
		acceptor_.bind(endpoint);
		acceptor_.listen();
		acceptor_.async_accept(connection_->socket(),
				boost::bind(&ConnectionTest::handle_accept, this,
						boost::asio::placeholders::error));
	}

	void client_connect()
	{
		boost::asio::ip::tcp::endpoint endpoint(
				boost::asio::ip::address_v4.from_string(address), port);

		// Try each endpoint until we successfully establish a connection.
		boost::asio::ip::tcp::socket socket(io_service_);
		socket.connect(endpoint, error);

		boost::asio::streambuf request;
		std::ostream request_stream(&request);
		request_stream << "test string";

		socket.write_some(request);
	}
};

TEST_F(ConnectionTest, Test1)
{
	server_start();
	client_connect();
	io_service_.run();
	ASSERT_EQ(11, bytes_transferred);
	ASSERT_EQ("test string", content);
}

} // namespace hainan
