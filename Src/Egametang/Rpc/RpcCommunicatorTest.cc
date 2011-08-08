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
	CountBarrier& barrier;
	std::string recv_message;
	RpcMeta recv_meta;
	boost::asio::ip::tcp::acceptor acceptor;

public:
	RpcServerTest(boost::asio::io_service& io_service, int port, CountBarrier& barrier):
		RpcCommunicator(io_service), acceptor(io_service),
		barrier(barrier)
	{
		boost::asio::ip::address address;
		address.from_string("127.0.0.1");
		boost::asio::ip::tcp::endpoint endpoint(address, port);
		acceptor.open(endpoint.protocol());
		acceptor.set_option(boost::asio::ip::tcp::acceptor::reuse_address(true));
		acceptor.bind(endpoint);
		acceptor.listen();
		acceptor.async_accept(socket,
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
		io_service.run();
	}

	void Stop()
	{
		acceptor.close();
		socket.close();
	}

	virtual void OnRecvMessage(RpcMetaPtr meta, StringPtr message)
	{
		VLOG(2) << "Server Recv string: " << *message;
		recv_message = *message;
		recv_meta = *meta;

		boost::hash<std::string> string_hash;

		RpcMetaPtr response_meta(new RpcMeta());
		StringPtr response_message(new std::string("response test rpc communicator string"));
		response_meta->size = response_message->size();
		response_meta->method = 123456;
		SendMeta(response_meta, response_message);

		barrier.Signal();
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
		socket.async_connect(endpoint,
				boost::bind(&RpcClientTest::OnAsyncConnect, this,
						boost::asio::placeholders::error));
	}

	void Start()
	{
		VLOG(2) << "Start Client";
		io_service.run();
	}

	void Stop()
	{
		socket.close();
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
	boost::asio::io_service io_server;
	boost::asio::io_service io_client;
	CountBarrier barrier_;
	RpcServerTest rpc_server;
	RpcClientTest rpc_client;

public:
	RpcCommunicatorTest():
		io_server(), io_client(),
		barrier_(2), rpc_server(io_server, global_port, barrier_),
		rpc_client(io_client, global_port, barrier_)
	{
	}

	virtual ~RpcCommunicatorTest()
	{
	}
};


TEST_F(RpcCommunicatorTest, SendAndRecvString)
{
	ThreadPool thread_pool(2);
	thread_pool.Schedule(boost::bind(&RpcServerTest::Start, &rpc_server));
	thread_pool.Schedule(boost::bind(&RpcClientTest::Start, &rpc_client));
	barrier_.Wait();
	thread_pool.Wait();
	rpc_server.Stop();
	rpc_client.Stop();

	ASSERT_EQ(std::string("send test rpc communicator string"), rpc_server.recv_message);
	ASSERT_EQ(rpc_server.recv_meta.size, rpc_server.recv_message.size());
	ASSERT_EQ(654321U, rpc_server.recv_meta.method);

	ASSERT_EQ(std::string("response test rpc communicator string"), rpc_client.recv_string_);
	ASSERT_EQ(rpc_client.meta_.size, rpc_client.recv_string_.size());
	ASSERT_EQ(123456U, rpc_client.meta_.method);
}

} // namespace Egametang


int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::ParseCommandLineFlags(&argc, &argv, true);
	google::InitGoogleLogging(argv[0]);
	return RUN_ALL_TESTS();
}
