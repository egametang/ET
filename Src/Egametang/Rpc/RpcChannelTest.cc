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
	RpcServerTest(boost::asio::io_service& io_service, int port, CountBarrier& barrier):
		RpcCommunicator(io_service), acceptor(io_service),
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

		StringPtr response_message(new std::string);
		response.SerializeToString(response_message.get());
		VLOG(3) << "response message: " << response_message->size();
		RpcMetaPtr response_meta(new RpcMeta());
		response_meta->id = meta->id;
		response_meta->size = response_message->size();
		SendMeta(response_meta, response_message);
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

static void IOServiceRun(boost::asio::io_service* io_service, CountBarrier* barrier)
{
	io_service->run();
	barrier->Signal();
}

TEST_F(RpcChannelTest, Echo)
{
	boost::asio::io_service io_server;
	boost::asio::io_service io_client;

	CountBarrier barrier(2);
	ThreadPool thread_pool(2);
	thread_pool.Schedule(boost::bind(&IOServiceRun, &io_server, &barrier));
	thread_pool.Schedule(boost::bind(&IOServiceRun, &io_client, &barrier));
	barrier.Wait();

	barrier.Reset(2);
	RpcServerTest rpc_server(io_server, port, barrier);
	RpcChannel rpc_channel(io_client, "127.0.0.1", port);
	EchoService_Stub service(&rpc_channel);

	EchoRequest request;
	request.set_num(100);

	EchoResponse response;

	ASSERT_EQ(0, response.num());
	service.Echo(NULL, &request, &response,
			google::protobuf::NewCallback(&barrier, &CountBarrier::Signal));
	barrier.Wait();
	rpc_channel.Stop();
	rpc_server.Stop();
	io_server.stop();
	io_client.stop();
	// rpc_channel是个无限循环的操作, 必须主动让channel和server stop才能wait线程
	thread_pool.Wait();

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
