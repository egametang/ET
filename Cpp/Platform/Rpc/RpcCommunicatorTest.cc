#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include <boost/threadpool.hpp>
#include <gtest/gtest.h>
#include <glog/logging.h>
#include <gflags/gflags.h>
#include "Rpc/RpcCommunicator.h"
#include "Thread/CountBarrier.h"

namespace Egametang {

static int globalPort = 10001;

class RpcServerTest: public RpcCommunicator
{
public:
	CountBarrier& barrier;
	std::string recvMessage;
	RpcMeta recvMeta;
	boost::asio::ip::tcp::acceptor acceptor;

public:
	RpcServerTest(boost::asio::io_service& ioService, int port, CountBarrier& barrier):
		RpcCommunicator(ioService), acceptor(ioService),
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
			return;
		}

		RpcMetaPtr meta(new RpcMeta());
		StringPtr message(new std::string);
		RecvMeta(meta, message);
	}

	void Start()
	{
		ioService.run();
	}

	void Stop()
	{
		acceptor.close();
		socket.close();
	}

	virtual void OnRecvMessage(RpcMetaPtr meta, StringPtr message)
	{
		recvMessage = *message;
		recvMeta = *meta;

		RpcMetaPtr responseMeta(new RpcMeta());
		StringPtr response_message(new std::string("response test rpc communicator string"));
		responseMeta->size = response_message->size();
		responseMeta->method = 123456;
		SendMeta(responseMeta, response_message);

		barrier.Signal();
	}
	virtual void OnSendMessage()
	{
	}
};

class RpcClientTest: public RpcCommunicator
{
public:
	CountBarrier& barrier;
	std::string recvString;
	RpcMeta recvMeta;

public:
	RpcClientTest(boost::asio::io_service& ioService, int port,
			CountBarrier& barrier):
		RpcCommunicator(ioService), barrier(barrier)
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
		ioService.run();
	}

	void Stop()
	{
		socket.close();
	}

	void OnAsyncConnect(const boost::system::error_code& err)
	{
		if (err)
		{
			return;
		}

		RpcMetaPtr sendMeta(new RpcMeta());
		StringPtr sendMessage(new std::string("send test rpc communicator string"));
		sendMeta->size = sendMessage->size();
		sendMeta->method = 654321;
		SendMeta(sendMeta, sendMessage);

		RpcMetaPtr meta(new RpcMeta());
		StringPtr message(new std::string);
		RecvMeta(meta, message);
	}

	virtual void OnRecvMessage(RpcMetaPtr meta, StringPtr message)
	{
		recvString = *message;
		recvMeta = *meta;
		barrier.Signal();
	}

	virtual void OnSendMessage()
	{
	}
};

class RpcCommunicatorTest: public testing::Test
{
protected:
	boost::asio::io_service ioServer;
	boost::asio::io_service ioClient;
	CountBarrier barrier;
	RpcServerTest rpcServer;
	RpcClientTest rpcClient;

public:
	RpcCommunicatorTest():
		ioServer(), ioClient(),
		barrier(2), rpcServer(ioServer, globalPort, barrier),
		rpcClient(ioClient, globalPort, barrier)
	{
	}

	virtual ~RpcCommunicatorTest()
	{
	}
};


TEST_F(RpcCommunicatorTest, SendAndRecvString)
{
	boost::threadpool::fifo_pool threadPool(2);
	threadPool.schedule(boost::bind(&RpcServerTest::Start, &rpcServer));
	threadPool.schedule(boost::bind(&RpcClientTest::Start, &rpcClient));
	barrier.Wait();
	threadPool.wait();
	rpcServer.Stop();
	rpcClient.Stop();

	ASSERT_EQ(std::string("send test rpc communicator string"), rpcServer.recvMessage);
	ASSERT_EQ(rpcServer.recvMeta.size, rpcServer.recvMessage.size());
	ASSERT_EQ(654321U, rpcServer.recvMeta.method);

	ASSERT_EQ(std::string("response test rpc communicator string"), rpcClient.recvString);
	ASSERT_EQ(rpcClient.recvMeta.size, rpcClient.recvString.size());
	ASSERT_EQ(123456U, rpcClient.recvMeta.method);
}

} // namespace Egametang


int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::InitGoogleLogging(argv[0]);
	google::ParseCommandLineFlags(&argc, &argv, true);
	return RUN_ALL_TESTS();
}
