#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>
#include "Rpc/RpcCommunicator.h"
#include "Thread/ThreadPool.h"
#include "Thread/CountBarrier.h"

namespace Egametang {

static int global_port = 10001;

class RpcServerTest: public RpcCommunicator
{
public:
	CountBarrier& barrier_;
	std::string recv_string_;
	RpcMeta meta_;
	boost::asio::ip::tcp::acceptor acceptor_;

public:
	RpcServerTest(boost::asio::io_service& io_service, int port, CountBarrier& barrier):
		RpcCommunicator(io_service), acceptor_(io_service),
		barrier_(barrier)
	{
		boost::asio::ip::address address;
		address.from_string("127.0.0.1");
		boost::asio::ip::tcp::endpoint endpoint(address, port);
		acceptor_.open(endpoint.protocol());
		acceptor_.set_option(boost::asio::ip::tcp::acceptor::reuse_address(true));
		acceptor_.bind(endpoint);
		acceptor_.listen();
		acceptor_.async_accept(socket_,
				boost::bind(&RpcServerTest::OnAsyncAccept, this,
						boost::asio::placeholders::error));
	}

	void OnAsyncAccept(const boost::system::error_code& err)
	{
		if (err)
		{
			LOG(ERROR) << "async accept failed: " << err.message();
			return;
		}

		RpcMetaPtr meta(new RpcMeta());
		StringPtr message(new std::string);
		RecvMeta(meta, message);
	}

	void Start()
	{
		VLOG(2) << "Start Server";
		io_service_.run();
	}

	void Stop()
	{
		acceptor_.close();
		socket_.close();
	}

	virtual void OnRecvMessage(RpcMetaPtr meta, StringPtr message)
	{
		VLOG(2) << "Server Recv string: " << *message;
		recv_string_ = *message;
		meta_ = *meta;

		boost::hash<std::string> string_hash;

		RpcMetaPtr response_meta(new RpcMeta());
		StringPtr response_message(new std::string("response test rpc communicator string"));
		response_meta->size = response_message->size();
		response_meta->method = 123456;
		SendMeta(response_meta, response_message);

		barrier_.Signal();
	}
	virtual void OnSendMessage()
	{
	}
};

class RpcClientTest: public RpcCommunicator
{
public:
	CountBarrier& barrier_;
	std::string recv_string_;
	RpcMeta meta_;

public:
	RpcClientTest(boost::asio::io_service& io_service, int port,
			CountBarrier& barrier):
		RpcCommunicator(io_service), barrier_(barrier)
	{
		boost::asio::ip::address address;
		address.from_string("127.0.0.1");
		boost::asio::ip::tcp::endpoint endpoint(address, port);
		socket_.async_connect(endpoint,
				boost::bind(&RpcClientTest::OnAsyncConnect, this,
						boost::asio::placeholders::error));
	}

	void Start()
	{
		VLOG(2) << "Start Client";
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
		boost::hash<std::string> string_hash;

		RpcMetaPtr send_meta(new RpcMeta());
		StringPtr send_message(new std::string("send test rpc communicator string"));
		send_meta->size = send_message->size();
		send_meta->method = 654321;
		SendMeta(send_meta, send_message);

		RpcMetaPtr meta(new RpcMeta());
		StringPtr message(new std::string);
		RecvMeta(meta, message);
	}

	virtual void OnRecvMessage(RpcMetaPtr meta, StringPtr message)
	{
		VLOG(2) << "Client Recv string: " << *message;
		recv_string_ = *message;
		meta_ = *meta;
		barrier_.Signal();
	}

	virtual void OnSendMessage()
	{
	}
};

class RpcCommunicatorTest: public testing::Test
{
protected:
	boost::asio::io_service io_server_;
	boost::asio::io_service io_client_;
	CountBarrier barrier_;
	RpcServerTest rpc_server_;
	RpcClientTest rpc_client_;

public:
	RpcCommunicatorTest():
		io_server_(), io_client_(),
		barrier_(2), rpc_server_(io_server_, global_port, barrier_),
		rpc_client_(io_client_, global_port, barrier_)
	{
	}

	virtual ~RpcCommunicatorTest()
	{
	}
};


TEST_F(RpcCommunicatorTest, SendAndRecvString)
{
	ThreadPool thread_pool(2);
	thread_pool.PushTask(boost::bind(&RpcServerTest::Start, &rpc_server_));
	thread_pool.PushTask(boost::bind(&RpcClientTest::Start, &rpc_client_));
	barrier_.Wait();
	thread_pool.Wait();
	rpc_server_.Stop();
	rpc_client_.Stop();

	ASSERT_EQ(std::string("send test rpc communicator string"), rpc_server_.recv_string_);
	ASSERT_EQ(rpc_server_.meta_.size, rpc_server_.recv_string_.size());
	ASSERT_EQ(654321U, rpc_server_.meta_.method);

	ASSERT_EQ(std::string("response test rpc communicator string"), rpc_client_.recv_string_);
	ASSERT_EQ(rpc_client_.meta_.size, rpc_client_.recv_string_.size());
	ASSERT_EQ(123456U, rpc_client_.meta_.method);
}

} // namespace Egametang


int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::ParseCommandLineFlags(&argc, &argv, true);
	google::InitGoogleLogging(argv[0]);
	return RUN_ALL_TESTS();
}
