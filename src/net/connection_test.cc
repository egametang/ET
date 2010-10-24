#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include "net/connection.h"

namespace hainan {

class connection1: public connection
{
public:
	string content;
	size_t bytes;
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
	asio::io_service io_service_;
	connection_set connections_;

	void SetUp()
	{
	}
};

TEST_F(ConnectionTest, Test1)
{

}

} // namespace hainan
