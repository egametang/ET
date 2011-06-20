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
		VLOG(3) << "server start accept";
		acceptor_.async_accept(socket_,
				boost::bind(&RPCServerTest::OnAsyncAccept, this,
						boost::asio::placeholders::error));
	}

	void OnAsyncAccept(const boost::system::error_code& err)
	{
		if (err)
		{
			LOG(ERROR) << "async accept failed: " << err.message();
			return;
		}
		RecvSize();
	}

	void Start()
	{
		io_service_.run();
	}

	void Stop()
	{
		socket_.close();
		acceptor_.close();
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

	void Stop()
	{
		socket_.close();
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
	boost::asio::io_service server_io_;
	boost::asio::io_service client_io_;
	RPCServerTest rpc_server_;
	RPCClientTest rpc_client_;

public:
	RPCCommunicatorTest():
		server_io_(), client_io_(),
		rpc_server_(server_io_, global_port),
		rpc_client_(client_io_, "localhost", global_port)
	{
	}
};


TEST_F(RPCCommunicatorTest, ClientSendString)
{
	VLOG(3) << "ClientSendString Test Start!";
	rpc_server_.Start();
	rpc_client_.Start();
}

} // namespace Egametang


int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::ParseCommandLineFlags(&argc, &argv, true);
	google::InitGoogleLogging(argv[0]);
	return RUN_ALL_TESTS();
}
