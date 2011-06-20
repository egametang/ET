#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>
#include "Rpc/RPCCommunicator.h"
#include "Thread/ThreadPool.h"
#include "Thread/CountBarrier.h"

namespace Egametang {

static int global_port = 10001;

class RPCServerTest: public RPCCommunicator
{
public:
	CountBarrier& barrier_;
	std::string recv_string_;
	boost::asio::ip::tcp::acceptor acceptor_;
	boost::asio::io_service io_service_;

public:
	RPCServerTest(int port, CountBarrier& barrier):
		barrier_(barrier), io_service_(),
		acceptor_(io_service_), RPCCommunicator(io_service_)
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
		barrier_.Signal();
	}
	virtual void OnSendMessage()
	{
	}
};

class RPCClientTest: public RPCCommunicator
{
public:
	CountBarrier& barrier_;
	std::string recv_string_;
	boost::asio::io_service io_service_;

public:
	RPCClientTest(std::string host, int port, CountBarrier& barrier):
		barrier_(barrier), io_service_(),
		RPCCommunicator(io_service_)
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
		barrier_.Signal();
	}

	virtual void OnSendMessage()
	{
	}
};

class RPCCommunicatorTest: public testing::Test
{
protected:
	CountBarrier barrier_;
	RPCServerTest rpc_server_;
	RPCClientTest rpc_client_;

public:
	RPCCommunicatorTest():
		barrier_(2),
		rpc_server_(global_port, barrier_),
		rpc_client_("localhost", global_port, barrier_)
	{
	}
};


TEST_F(RPCCommunicatorTest, ClientSendString)
{
	VLOG(3) << "ClientSendString Test Start!";
	ThreadPool thread_pool(3);
	thread_pool.PushTask(boost::bind(&RPCServerTest::Start, &rpc_server_));
	thread_pool.PushTask(boost::bind(&RPCClientTest::Start, &rpc_client_));
	barrier_.Wait();
	ASSERT_EQ(std::string("send test rpc communicator string"), rpc_server_.recv_string_);
	ASSERT_EQ(std::string("send test rpc communicator string"), rpc_client_.recv_string_);
	rpc_server_.Stop();
	rpc_client_.Stop();
}

} // namespace Egametang


int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::ParseCommandLineFlags(&argc, &argv, true);
	google::InitGoogleLogging(argv[0]);
	return RUN_ALL_TESTS();
}
