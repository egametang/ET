#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>
#include "Rpc/RPCCommunicator.h"

namespace Egametang {

static int global_port = 10001;

class RPCServerTest: public RPCCommunicator
{
public:
	std::string recv_string_;
	boost::asio::ip::tcp::acceptor acceptor_;
	boost::asio::io_service& io_service_;

public:
	RPCServerTest(boost::asio::io_service& io_service, int port):
		io_service_(io_service), acceptor_(io_service_),
		RPCCommunicator(io_service_)
	{
		boost::asio::ip::address address;
		address.from_string("localhost");
		boost::asio::ip::tcp::endpoint endpoint(address, port);
		acceptor_.open(endpoint.protocol());
		acceptor_.set_option(boost::asio::ip::tcp::acceptor::reuse_address(true));
		acceptor_.bind(endpoint);
		acceptor_.listen();
		acceptor_.async_accept(socket_,
				boost::bind(&RPCServerTest::OnAsyncAccept, this,
						boost::asio::placeholders::error));
	}

	void OnAsyncAccept(const boost::system::error_code& err)
	{
	}

	void Start()
	{
		RecvSize();
	}

	virtual void OnRecvMessage(StringPtr ss)
	{
		recv_string_ = *ss;
	}
	virtual void OnSendMessage()
	{
	}
};

class RPCClientTest: public RPCCommunicator
{
public:
	std::string recv_string_;
	boost::asio::io_service& io_service_;

public:
	RPCClientTest(boost::asio::io_service& io_service, std::string host, int port):
		io_service_(io_service), RPCCommunicator(io_service_)
	{
		boost::asio::ip::address address;
		address.from_string(host);
		boost::asio::ip::tcp::endpoint endpoint(address, port);
		socket_.async_connect(endpoint,
				boost::bind(&RPCClientTest::OnAsyncConnect, this,
						boost::asio::placeholders::error));
	}

	void Start()
	{
		io_service_.run();
	}

	void OnAsyncConnect(const boost::system::error_code& err)
	{
		if (err)
		{
			LOG(ERROR) << "async connect failed: " << err.message();
			return;
		}
		std::string send_string("send test rpc communicator string");
		SendSize(send_string.size(), send_string);
	}

	virtual void OnRecvMessage(StringPtr ss)
	{
		recv_string_ = *ss;
	}

	virtual void OnSendMessage()
	{
	}
};

class RPCCommunicatorTest: public testing::Test
{
protected:
	boost::asio::io_service io_service_;
	RPCServerTest rpc_server_;
	RPCClientTest rpc_client_;

public:
	RPCCommunicatorTest():
		io_service_(), rpc_server_(io_service_, global_port),
		rpc_client_(io_service_, "localhost", global_port)
	{
	}
};


TEST_F(RPCCommunicatorTest, ClientSendString)
{
	int a = 2;
}

} // namespace Egametang


int main(int argc, char* argv[])
{
	FLAGS_logtostderr = true;
	testing::InitGoogleTest(&argc, argv);
	google::ParseCommandLineFlags(&argc, &argv, true);
	google::InitGoogleLogging(argv[0]);
	return RUN_ALL_TESTS();
}
