#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>
#include "Rpc/RpcSession.h"
#include "Rpc/RpcServerMock.h"

namespace Egametang {

class RpcSessionTest: public testing::Test
{
protected:
	boost::asio::io_service io_service;
	int port;
	RpcServerMock mock_server;
	RpcSession session;

public:
	RpcSessionTest():
		io_service(), port(10000),
		mock_server(port), session(io_service, mock_server)
	{
	}

	virtual ~RpcSessionTest()
	{
	}
};

} // namespace Egametang


int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::ParseCommandLineFlags(&argc, &argv, true);
	google::InitGoogleLogging(argv[0]);
	return RUN_ALL_TESTS();
}
