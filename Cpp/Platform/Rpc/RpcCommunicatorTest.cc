#include <boost/asio.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/make_shared.hpp>
#include <gtest/gtest.h>
#include "Rpc/RpcCommunicator.h"
#include "Thread/CountBarrier.h"
#include "Log/Log.h"

namespace Egametang {

static int globalPort = 11111;

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

		auto meta = boost::make_shared<RpcMeta>();
		auto message = boost::make_shared<std::string>();
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

		auto responseMeta = boost::make_shared<RpcMeta>();
		auto response_message = boost::make_shared<std::string>(
				"response test rpc communicator string");
		responseMeta->size = response_message->size();
		responseMeta->method = 123456;
		SendMeta(responseMeta, response_message);

		barrier.Signal();
	}
};

class RpcClientTest: public RpcCommunicator
{
public:
	CountBarrier& barrier;
	std::string recvString;
	RpcMeta recvMeta;

public:
	RpcClientTest(boost::asio::io_service& ioService, int port, CountBarrier& barrier):
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
			LOG(INFO) << "async connect error: " << err.message();
			return;
		}

		auto sendMeta = boost::make_shared<RpcMeta>();
		auto sendMessage = boost::make_shared<std::string>(
				"send test rpc communicator string");
		sendMeta->size = sendMessage->size();
		sendMeta->method = 654321;
		SendMeta(sendMeta, sendMessage);

		auto meta = boost::make_shared<RpcMeta>();
		auto message = boost::make_shared<std::string>();
		RecvMeta(meta, message);
	}

	virtual void OnRecvMessage(RpcMetaPtr meta, StringPtr message)
	{
		recvString = *message;
		recvMeta = *meta;
		barrier.Signal();
	}
};

class RpcCommunicatorTest: public testing::Test
{
};


TEST_F(RpcCommunicatorTest, SendAndRecvString)
{
	boost::asio::io_service ioServer;
	boost::asio::io_service ioClient;

	CountBarrier barrier(2);

	RpcServerTest rpcServer(ioServer, globalPort, barrier);
	boost::thread serverThread(boost::bind(&RpcServerTest::Start, &rpcServer));

	boost::this_thread::sleep(boost::posix_time::milliseconds(1000));

	RpcClientTest rpcClient(ioClient, globalPort, barrier);
	boost::thread clientThread(boost::bind(&RpcClientTest::Start, &rpcClient));
	barrier.Wait();

	ioClient.post(boost::bind(&boost::asio::io_service::stop, &ioClient));
	ioServer.post(boost::bind(&boost::asio::io_service::stop, &ioServer));

	serverThread.join();
	clientThread.join();

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
	Egametang::Log::Init(argv[0]);
	return RUN_ALL_TESTS();
}
