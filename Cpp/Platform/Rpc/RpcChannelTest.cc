#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>
#include "Rpc/RpcChannel.h"
#include "Thread/CountBarrier.h"
#include "Thread/ThreadPool.h"
#include "Rpc/RpcController.h"
#include "Rpc/Echo.pb.h"

namespace Egametang {

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

	void Stop()
	{
		acceptor.close();
		socket.close();
	}

	virtual void OnRecvMessage(RpcMetaPtr meta, StringPtr message)
	{
		// 接收消息
		EchoRequest request;
		request.ParseFromString(*message);

		num = request.num();

		// 回一个消息
		EchoResponse response;
		response.set_num(num);

		StringPtr responseMessage(new std::string);
		response.SerializeToString(responseMessage.get());
		VLOG(3) << "response message: " << responseMessage->size();
		RpcMetaPtr responseMeta(new RpcMeta());
		responseMeta->id = meta->id;
		responseMeta->size = responseMessage->size();
		SendMeta(responseMeta, responseMessage);
	}
	virtual void OnSendMessage(RpcMetaPtr meta, StringPtr message)
	{
		barrier.Signal();
	}
};

class RpcChannelTest: public testing::Test
{
protected:
	int port;

public:
	RpcChannelTest(): port(10002)
	{
	}
	virtual ~RpcChannelTest()
	{
	}
};

static void IOServiceRun(boost::asio::io_service* ioService)
{
	ioService->run();
}

TEST_F(RpcChannelTest, Echo)
{
	boost::asio::io_service ioServer;
	boost::asio::io_service ioClient;

	CountBarrier barrier(2);
	RpcServerTest server(ioServer, port, barrier);
	RpcChannelPtr channel(new RpcChannel(ioClient, "127.0.0.1", port));
	EchoService_Stub service(channel.get());

	ThreadPool threadPool(2);
	threadPool.Schedule(boost::bind(&IOServiceRun, &ioServer));
	threadPool.Schedule(boost::bind(&IOServiceRun, &ioClient));

	EchoRequest request;
	request.set_num(100);

	EchoResponse response;

	ASSERT_EQ(0, response.num());
	service.Echo(NULL, &request, &response,
			google::protobuf::NewCallback(&barrier, &CountBarrier::Signal));
	barrier.Wait();
	channel->Stop();
	server.Stop();
	ioServer.stop();
	ioClient.stop();
	// rpc_channel是个无限循环的操作, 必须主动让channel和server stop才能wait线程
	threadPool.Wait();

	ASSERT_EQ(100, response.num());
}

} // namespace Egametang


int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::ParseCommandLineFlags(&argc, &argv, true);
	google::InitGoogleLogging(argv[0]);
	return RUN_ALL_TESTS();
}
