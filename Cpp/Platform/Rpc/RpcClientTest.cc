#include <boost/threadpool.hpp>
#include <gtest/gtest.h>
#include "Rpc/RpcClient.h"
#include "Thread/CountBarrier.h"
#include "Rpc/RpcController.h"
#include "Rpc/Echo.pb.h"

namespace Egametang {

static int globalPort = 10000;

class RpcServerTest: public RpcCommunicator
{
public:
	CountBarrier& barrier;
	int32 num;
	boost::asio::ip::tcp::acceptor acceptor;

public:
	RpcServerTest(boost::asio::io_service& ioService, int port, CountBarrier& barrier):
		RpcCommunicator(ioService), acceptor(ioService),
		barrier(barrier), num(0)
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
	~RpcServerTest()
	{
		acceptor.close();
		socket.close();
	}

	void OnAsyncAccept(const boost::system::error_code& err)
	{
		if (err)
		{
			return;
		}
		auto meta = std::make_shared<RpcMeta>();
		auto message = std::make_shared<std::string>();
		RecvMeta(meta, message);
	}

	virtual void OnRecvMessage(RpcMetaPtr meta, StringPtr message)
	{
		EchoRequest request;
		request.ParseFromString(*message);

		num = request.num();

		EchoResponse response;
		response.set_num(num);

		auto responseMessage = std::make_shared<std::string>();
		response.SerializeToString(responseMessage.get());
		auto responseMeta = std::make_shared<RpcMeta>();
		responseMeta->id = meta->id;
		responseMeta->size = responseMessage->size();
		SendMeta(responseMeta, responseMessage);
	}

	virtual void OnSendMessage(RpcMetaPtr meta, StringPtr message)
	{
		barrier.Signal();
	}
};

class RpcClientTest: public testing::Test
{
};

static void IOServiceRun(boost::asio::io_service* ioService)
{
	ioService->run();
}

TEST_F(RpcClientTest, Echo)
{
	boost::asio::io_service ioServer;
	boost::asio::io_service ioClient;

	CountBarrier barrier(2);
	RpcServerTest server(ioServer, globalPort, barrier);
	auto client = std::make_shared<RpcClient>(ioClient, "127.0.0.1", globalPort);
	EchoService_Stub service(client.get());

	boost::threadpool::fifo_pool threadPool(2);
	threadPool.schedule(boost::bind(&IOServiceRun, &ioServer));

	boost::this_thread::sleep(boost::posix_time::milliseconds(500));
	threadPool.schedule(boost::bind(&IOServiceRun, &ioClient));

	EchoRequest request;
	request.set_num(100);

	EchoResponse response;

	ASSERT_EQ(0, response.num());
	service.Echo(nullptr, &request, &response,
			google::protobuf::NewCallback(&barrier, &CountBarrier::Signal));
	barrier.Wait();

	// 加入任务队列,等client和server stop,io_service才stop
	ioClient.post(boost::bind(&boost::asio::io_service::stop, &ioClient));
	ioServer.post(boost::bind(&boost::asio::io_service::stop, &ioServer));
	ASSERT_EQ(100, response.num());
}

} // namespace Egametang


int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	return RUN_ALL_TESTS();
}
